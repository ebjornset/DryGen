using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DryGen.Docs;

[ExcludeFromCodeCoverage] // We run this from nuke docs, so we are not to worried about the code coverage at the moment...
public static class Program
{
	public static int Main(string[] args)
	{
		return Parser.Default.ParseArguments<Options>(args)
			.MapResult(
			options => RunAndReturnExitCode(options),
			_ => 1);
	}

	private static int RunAndReturnExitCode(Options options)
	{
		var rootDirectory = Path.GetFullPath(options.RootDirectory);
		if (!Directory.Exists(rootDirectory))
		{
			throw new ArgumentException($"Root directory '{rootDirectory}' does not exist!");
		}
		GenerateVerbsToc(rootDirectory);
		GenerateVerbsMarkdown(rootDirectory);
		GenerateExamplesToc(rootDirectory);
		GenerateExamplesFilesFromTemplates(rootDirectory);
		GenerateReleaseNotestToc(rootDirectory);
		GenerateReleasNotestFilesFromTemplates(rootDirectory);
		var result = 0;
		string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var generator = new Generator(Console.Out, Console.Error, useAssemblyLoadContextDefault: true);
		foreach (var generatorData in BuildExamplesGeneratorData())
		{
			result = ReplaceRepresentationInExample(rootDirectory, assemblyDirectory, generator, generatorData);
			if (result > 0)
			{
				break;
			}
			ReplaceCommandlineInExample(rootDirectory, assemblyDirectory, generator, generatorData);
		}
		GenerateErDigramExampleCodeInIncludeExampleCs(rootDirectory);
		return result;
	}

	private static int ReplaceRepresentationInExample(string rootDirectory, string assemblyDirectory, Generator generator, ExamplesGeneratorData generatorData)
	{
		int result;
		var commandline = BuildExamplesGeneratorCommandline(generatorData, rootDirectory, assemblyDirectory).ToList();
		commandline.Add($"--{Constants.OutputTemplate}");
		commandline.Add(GetOutputTemplate());
		Console.WriteLine($"Generating: {string.Join(' ', commandline)}");
		result = generator.Run(commandline.ToArray());
		return result;

		static string GetOutputTemplate()
		{
			return """
			### The resulting Mermaid diagram

			```mermaid
			${DryGenOutput}
			``` 

			### The Mermaid code behind the diagram

			```
			${DryGenOutput}
			```

			""";
		}
	}

	private static void ReplaceCommandlineInExample(string rootDirectory, string assemblyDirectory, Generator generator, ExamplesGeneratorData generatorData)
	{
		var commandline = BuildExamplesGeneratorCommandline(generatorData, rootDirectory, assemblyDirectory);
		var examplesCommandLine = $"dry-gen {string.Join(' ', commandline)}";
		var existingRepresentation = generator.ReadExistingRepresentationFromOutputFileAndValidateReplaceToken(generatorData.Verb, GetGeneratedExamplesOutputFile(rootDirectory, generatorData), generatorData.ReplaceToken.AsCommandLineReplaceToken(), verbose: false);
		var examplesCommandLineRepresentation = Generator.BuildResultReprersentationFromOutputTemplate(examplesCommandLine, GetOutputTemplate());
		var generatedRepresentation = existingRepresentation.Replace(generatorData.ReplaceToken.AsCommandLineReplaceToken(), examplesCommandLineRepresentation);
		File.WriteAllText(GetGeneratedExamplesOutputFile(rootDirectory, generatorData), generatedRepresentation);

		static string GetOutputTemplate()
		{
			return """
			### The commandline

			`${DryGenOutput}`
				
			""";
		}
	}

	private static string GetGeneratedExamplesOutputFile(string rootDirectory, ExamplesGeneratorData generatorData)
	{
		return Path.Combine(rootDirectory.AsGeneratedExamplesDirectoryCreated(), generatorData.OutputFile.ToLowerInvariant()).AsLinuxPath();
	}

	private static string GetGeneratedIncludeExamplesOutputFile(string rootDirectory, ExamplesGeneratorData generatorData)
	{
		return Path.Combine(rootDirectory.AsGeneratedIncludeExamplesDirectoryCreated(), generatorData.OutputFile.ToLowerInvariant()).AsLinuxPath();
	}

	private static void GenerateVerbsMarkdown(string rootDirectory)
	{
		var verbs = typeof(Generator).Assembly.GetTypes().Where(x => x.HasVerbAttribute()).Select(x => x.GetVerb());
		foreach (var verb in verbs.OrderBy(x => x))
		{
			var verbMarkdownPath = Path.Combine(rootDirectory.AsGeneratedVerbsDirectoryCreated(), $"{verb}.md").AsLinuxPath();
			Console.WriteLine($"Generating verb markdown for '{verb}' to \"{verbMarkdownPath}\"");
			using var verbMarkdownWriter = new StreamWriter(verbMarkdownPath);
			VerbMarkdowGenerator.Generate(verb, verbMarkdownWriter);
		}
	}

	private static void GenerateVerbsToc(string rootDirectory)
	{
		GenerateToc(rootDirectory.AsGeneratedVerbsDirectoryCreated(), "verbs", tocWriter => VerbTocGenerator.Generate(tocWriter));
	}

	private static void GenerateReleaseNotestToc(string rootDirectory)
	{
		var releaseNotestTemplateDirectory = rootDirectory.AsTemplatesReleaseNotesDirectory();
		GenerateToc(rootDirectory.AsGeneratedReleaseNotesDirectoryCreated(), "release notes", tocWriter => ReleaseNotesTocGenerator.Generate(tocWriter, releaseNotestTemplateDirectory));
	}

	private static void GenerateExamplesToc(string rootDirectory)
	{
		var examplesTemplateDirectory = rootDirectory.AsTemplatesExamplesDirectory();
		GenerateToc(rootDirectory.AsGeneratedExamplesDirectoryCreated(), "examples", tocWriter => ExamplesTocGenerator.Generate(tocWriter, examplesTemplateDirectory));
	}

	private static void GenerateToc(string tocDirectory, string tocName, Action<TextWriter> tocBuilder)
	{
		var tocPath = Path.Combine(tocDirectory, "toc.yml").AsLinuxPath();
		Console.WriteLine($"Generating {tocName} TOC to \"{tocPath}\"");
		using var tocWriter = new StreamWriter(tocPath);
		tocBuilder(tocWriter);
	}

	private static void GenerateExamplesFilesFromTemplates(string rootDirectory)
	{
		var examplesTemplatesDirectory = rootDirectory.AsTemplatesExamplesDirectory();
		var examplesDirectory = rootDirectory.AsGeneratedExamplesDirectoryCreated();
		foreach (var exampleTemplateFile in Directory.GetFiles(examplesTemplatesDirectory).Select(x => Path.GetFileName(x)))
		{
			Console.WriteLine($"Generating examples from template file \"{exampleTemplateFile}\" in directory \"{examplesTemplatesDirectory}\" to \"{examplesDirectory}\"");
			ExamplesFileGenerator.Generate(rootDirectory, exampleTemplateFile);
		}
	}

	private static void GenerateReleasNotestFilesFromTemplates(string rootDirectory)
	{
		var releaseNotesTemplatesDirectory = rootDirectory.AsTemplatesReleaseNotesDirectory();
		var releaseNotesDirectory = rootDirectory.AsGeneratedReleaseNotesDirectoryCreated();
		foreach (var releaseNotesTemplateFile in Directory.GetFiles(releaseNotesTemplatesDirectory).Select(x => Path.GetFileName(x)))
		{
			Console.WriteLine($"Generating release notes from template file \"{releaseNotesTemplateFile}\" in directory \"{releaseNotesTemplatesDirectory}\" to \"{releaseNotesDirectory}\"");
			ReleaseNotesFileGenerator.Generate(rootDirectory, releaseNotesTemplateFile);
		}
	}

	private static IEnumerable<ExamplesGeneratorData> BuildExamplesGeneratorData()
	{
		return new[]
		{
			GetExamplesGeneratorDataForFilteringMermaidDiagramContent("no-filtering", new[] {"--attribute-level", "none", "--method-level", "none", "--direction", "RL" }),
			GetExamplesGeneratorDataForFilteringMermaidDiagramContent("tree-shaking-roots", new[] {"--tree-shaking-roots", "^ITypeFilter$", "--attribute-level", "none", "--method-level", "none", "--direction", "RL" }),
			GetExamplesGeneratorDataForFilteringMermaidDiagramContent("include-namespaces", new[] { "--include-namespaces", "^DryGen.MermaidFromCSharp.ClassDiagram$", "--attribute-level", "none", "--method-level", "none", "--direction", "TB" }),
			GetExamplesGeneratorDataForFilteringMermaidDiagramContent("include-typenames", new[] { "--include-typenames", ".*ClassDiagram.*", "--attribute-level", "none", "--method-level", "none", "--direction", "TB" }),
			GetExamplesGeneratorDataForFilteringMermaidDiagramContent("exclude-typenames", new[] { "--exclude-typenames", ".*TypeFilter.*;.*ClassDiagram.*", "--attribute-level", "none", "--method-level", "none", "--direction", "RL" }),

			GetExamplesGeneratorDataForMermaidClassDiagramDetails("no-filtering", new[] {"--include-typenames", "^ClassDiagramGenerator$"}),
			GetExamplesGeneratorDataForMermaidClassDiagramDetails("exclude-static-methods", new[] { "--include-typenames", "^ClassDiagramGenerator$", "--exclude-static-methods", "true"}),
			GetExamplesGeneratorDataForMermaidClassDiagramDetails("exclude-method-params", new[] { "--include-typenames", "^ClassDiagramGenerator$", "--exclude-method-params", "true"}),
			GetExamplesGeneratorDataForMermaidClassDiagramDetails("method-level", new[] { "--include-typenames", "^ClassDiagramGenerator$", "--method-level", "public"}),
			GetExamplesGeneratorDataForMermaidClassDiagramDetails("name-replace", new[] { "--name-replace-from", "ClassDiagram", "--name-replace-to", "", "--include-typenames", "^ClassDiagram.*", "--attribute-level", "none", "--method-level", "none", "--direction", "TB"}),

			GetExamplesGeneratorDataForMermaidErDiagramDetails("no-filtering", null),
			GetExamplesGeneratorDataForMermaidErDiagramDetails("attribute-type-exclusion", new [] { "--attribute-type-exclusion", "foreignkeys" }),
			GetExamplesGeneratorDataForMermaidErDiagramDetails("relationship-type-exclusion", new [] { "--relationship-type-exclusion", "all" }),
			GetExamplesGeneratorDataForMermaidErDiagramDetails("exclude-attribute-keytypes", new [] { "--exclude-attribute-keytypes", "true" }),
			GetExamplesGeneratorDataForMermaidErDiagramDetails("exclude-attribute-comments", new [] { "--exclude-attribute-comments", "true" }),
			GetExamplesGeneratorDataForMermaidErDiagramDetails("exclude-propertynames", new [] { "--exclude-propertynames", ".*Id$" }),
		};
	}

	private static string[] BuildExamplesGeneratorCommandline(ExamplesGeneratorData generatorData, string rootDirectory, string assemblyDirectory)
	{
		var inputFile = Path.GetRelativePath(rootDirectory, Path.Combine(assemblyDirectory, generatorData.InputFile)).AsLinuxPath();
		var outputFile = Path.GetRelativePath(rootDirectory, GetGeneratedExamplesOutputFile(rootDirectory, generatorData)).AsLinuxPath();
		var result = new List<string>
		{
			generatorData.Verb,
			$"--{Constants.InputFileOption}",
			inputFile,
			$"--{Constants.OutputFileOption}",
			outputFile,
			$"--{Constants.ReplaceTokenInOutputFile}",
			generatorData.ReplaceToken.AsGeneratedRepresentationReplaceToken(),
		};
		if (generatorData.AdditionalOptions?.Any() == true)
		{
			result.AddRange(generatorData.AdditionalOptions);
		}
		return result.ToArray();
	}

	private static void GenerateErDigramExampleCodeInIncludeExampleCs(string rootDirectory)
	{
		var generatorData = new ExamplesGeneratorData
		{
			OutputFile = "example.cs",
		};
		var exampleCodeFile = Path.Combine(rootDirectory, "src", "DryGen.Templates", "templates", "DryGen.Templates.Mermaid", "Example.cs");
		var generatedRepresentation = File.ReadAllText(exampleCodeFile).Replace("[ExcludeFromCodeCoverage(Justification = \"Just example code\")]", null).Replace("using System.Diagnostics.CodeAnalysis;", null);
		File.WriteAllText(GetGeneratedIncludeExamplesOutputFile(rootDirectory, generatorData), generatedRepresentation);
	}

	private static ExamplesGeneratorData GetExamplesGeneratorDataForFilteringMermaidDiagramContent(string replaceTopic, string[] additionalOptions)
	{
		return GetExamplesGeneratorData(
			replaceTopic,
			additionalOptions,
			verb: Constants.MermaidClassDiagramFromCsharp.Verb,
			inputFile: "DryGen.MermaidFromCSharp.dll",
			outputFile: "filtering-mermaid-diagram-content.md",
			replaceTokenPrefix: "mermaid-diagram-filter-example-");
	}

	private static ExamplesGeneratorData GetExamplesGeneratorDataForMermaidClassDiagramDetails(string replaceTopic, string[] additionalOptions)
	{
		return GetExamplesGeneratorData(
			replaceTopic,
			additionalOptions,
			verb: Constants.MermaidClassDiagramFromCsharp.Verb,
			inputFile: "DryGen.MermaidFromCSharp.dll",
			outputFile: "mermaid-class-diagram-details.md",
			replaceTokenPrefix: "mermaid-class-diagram-details-example-");
	}

	private static ExamplesGeneratorData GetExamplesGeneratorDataForMermaidErDiagramDetails(string replaceTopic, string[] additionalOptions)
	{
		return GetExamplesGeneratorData(
			replaceTopic,
			additionalOptions,
			verb: Constants.MermaidErDiagramFromEfCore.Verb,
			inputFile: "DryGen.Docs.dll",
			outputFile: "mermaid-er-diagram-details.md",
			replaceTokenPrefix: "mermaid-er-diagram-details-example-");
	}

	private static ExamplesGeneratorData GetExamplesGeneratorData(
		string replaceTopic,
		string[] additionalOptions,
		string verb,
		string inputFile,
		string outputFile,
		string replaceTokenPrefix)
	{
		var result = new ExamplesGeneratorData
		{
			Verb = verb,
			InputFile = inputFile,
			OutputFile = outputFile,
			ReplaceToken = $"{replaceTokenPrefix}{replaceTopic}",
		};
		if (additionalOptions != null)
		{
			result.AdditionalOptions = additionalOptions;
		}
		return result;
	}

	[ExcludeFromCodeCoverage]
	internal class Options
	{
		[Option("root-directory", Required = true, HelpText = "Sets the root directory, assuming this is the parent directory of the docs directory where stuff will be generated.")]
		public string RootDirectory { get; set; }
	}
}