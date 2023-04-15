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

    static int RunAndReturnExitCode(Options options)
    {
        var rootDirectory = Path.GetFullPath(options.RootDirectory);
        if (!Directory.Exists(rootDirectory))
        {
            throw new ArgumentException($"Root directory '{rootDirectory}' does not exist!");
        }
        GenerateVerbsMenu(rootDirectory);
        GenerateVerbsMarkdown(rootDirectory);
        GenerateExamplesMenu(rootDirectory);
        GenerateExamplesFilesFromTemplates(rootDirectory);
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
        ReplaceErDigramExampleCodeInExample(rootDirectory, generator);
        return result;
    }

    private static int ReplaceRepresentationInExample(string rootDirectory, string assemblyDirectory, Generator generator, ExamplesGeneratorData generatorData)
    {
        int result;
        var commandline = BuildExamplesGeneratorCommandline(generatorData, rootDirectory, assemblyDirectory);
        Console.WriteLine($"Generating: {string.Join(' ', commandline)}");
        result = generator.Run(commandline);
        return result;
    }

    private static void ReplaceCommandlineInExample(string rootDirectory, string assemblyDirectory, Generator generator, ExamplesGeneratorData generatorData)
    {
        var commandline = BuildExamplesGeneratorCommandline(generatorData, rootDirectory, assemblyDirectory);
        var examplesCommandLine = $"dry-gen {string.Join(' ', commandline)}";
        var existingRepresentation = generator.ReadExistingRepresentationFromOutputFileAndValidateReplaceToken(generatorData.Verb, GetOutputFile(rootDirectory, generatorData), generatorData.ReplaceToken.AsCommandLineReplaceToken(), verbose: false);
        var generatedRepresentation = existingRepresentation.Replace(generatorData.ReplaceToken.AsCommandLineReplaceToken(), examplesCommandLine);
        File.WriteAllText(GetOutputFile(rootDirectory, generatorData), generatedRepresentation);
    }

    private static string GetOutputFile(string rootDirectory, ExamplesGeneratorData generatorData)
    {
        return Path.Combine(rootDirectory.AsExamplesDirectory(), generatorData.OutputFile.ToLowerInvariant()).AsLinuxPath();
    }

    private static void GenerateVerbsMarkdown(string rootDirectory)
    {
        var verbs = typeof(Generator).Assembly.GetTypes().Where(x => x.HasVerbAttribute()).Select(x => x.GetVerb());
        foreach (var verb in verbs.OrderBy(x => x))
        {
            var verbMarkdownPath = Path.Combine(rootDirectory.AsVerbsDirectory(), $"{verb}.md").AsLinuxPath();
            Console.WriteLine($"Generating verb markdown for '{verb}' to \"{verbMarkdownPath}\"");
            using var verbMarkdownWriter = new StreamWriter(verbMarkdownPath);
            VerbMarkdowGenerator.Generate(verb, verbMarkdownWriter);
        }
    }

    private static void GenerateVerbsMenu(string rootDirectory)
    {
        var verbMenuPath = Path.Combine(rootDirectory.AsDataDirectory(), "verbs_menu.yml").AsLinuxPath();
        Console.WriteLine($"Generating verbs menu to \"{verbMenuPath}\"");
        using var verbMenuWriter = new StreamWriter(verbMenuPath);
        VerbMenuGenerator.Generate(verbMenuWriter);
    }

    private static void GenerateExamplesMenu(string rootDirectory)
    {
        var examplesTemplateDirectory = rootDirectory.AsExamplesTemplatesDirectory();
        var examplesMenuPath = Path.Combine(rootDirectory.AsDataDirectory(), "examples_menu.yml").AsLinuxPath();
        Console.WriteLine($"Generating examples menu to \"{examplesMenuPath}\"");
        using var examplesMenuWriter = new StreamWriter(examplesMenuPath);
        ExamplesMenuGenerator.Generate(examplesMenuWriter, examplesTemplateDirectory);
    }

    private static void GenerateExamplesFilesFromTemplates(string rootDirectory)
    {
        var examplesTemplatesDirectory = rootDirectory.AsExamplesTemplatesDirectory();
        var examplesDirectory = rootDirectory.AsExamplesDirectory();
        foreach (var exampleTemplateFile in Directory.GetFiles(examplesTemplatesDirectory).Select(x => Path.GetFileName(x)))
        {
            Console.WriteLine($"Generating examples from template file \"{exampleTemplateFile}\" in directory \"{examplesTemplatesDirectory}\" to \"{examplesDirectory}\"");
            ExamplesFileGenerator.Generate(rootDirectory, exampleTemplateFile);
        }
    }

    private static IEnumerable<ExamplesGeneratorData> BuildExamplesGeneratorData()
    {
        return new[] {
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
        var inputFile = Path.GetRelativePath(rootDirectory, Path.Combine(assemblyDirectory, generatorData.InputFile)).Replace("\\", "/");
        var outputFile = Path.GetRelativePath(rootDirectory, GetOutputFile(rootDirectory, generatorData)).Replace("\\", "/");
        var result = new List<string> {
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

    private static void ReplaceErDigramExampleCodeInExample(string rootDirectory, Generator generator)
    {
        var generatorData = new ExamplesGeneratorData
        {
            OutputFile = "mermaid-er-diagram-details.md",
            ReplaceToken = "mermaid-er-diagram-details-example-code",
        };
        var exampleCodeFile = Path.Combine(rootDirectory, "src", "DryGen.Templates", "templates", "DryGen.Templates.Mermaid", "Example.cs");
        var exampleCode = File.ReadAllText(exampleCodeFile).Replace("[ExcludeFromCodeCoverage(Justification = \"Just example code\")]", string.Empty).Replace("using System.Diagnostics.CodeAnalysis;", string.Empty);
        var replaceToken = generatorData.ReplaceToken.AsGeneratedRepresentationReplaceToken();
        var existingRepresentation = generator.ReadExistingRepresentationFromOutputFileAndValidateReplaceToken(generatorData.Verb, GetOutputFile(rootDirectory, generatorData), replaceToken, verbose: false);
        var generatedRepresentation = existingRepresentation.Replace(replaceToken, exampleCode);
        File.WriteAllText(GetOutputFile(rootDirectory, generatorData), generatedRepresentation);
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
