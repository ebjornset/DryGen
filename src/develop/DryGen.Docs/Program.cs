using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DryGen.Docs
{
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
            var docsDirectory = Path.GetFullPath(options.DocsDirectory);
            if (!Directory.Exists(docsDirectory))
            {
                throw new ArgumentException($"Docs directory '{docsDirectory}' does not exist!");
            }
            GenerateVerbsMenu(docsDirectory);
            GenerateVerbsMarkdown(docsDirectory);
            GenerateExamplesMenu(docsDirectory);
            CopyExamplesTemplates(docsDirectory);
            var result = 0;
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var generator = new Generator(Console.Out, Console.Error);
            foreach (var generatorData in BuildExamplesGeneratorData())
            {
                result = ReplaceRepresentationInExample(docsDirectory, assemblyDirectory, generator, generatorData);
                if (result > 0)
                {
                    break;
                }
                ReplaceCommandlineInExample(docsDirectory, assemblyDirectory, generator, generatorData);
            }
            return result;
        }

        private static int ReplaceRepresentationInExample(string docsDirectory, string assemblyDirectory, Generator generator, ExamplesGeneratorData generatorData)
        {
            int result;
            var commandline = BuildExamplesGeneratorCommandline(generatorData, docsDirectory, assemblyDirectory, relativeRoot: null);
            Console.WriteLine($"Generating: {string.Join(' ', commandline)}");
            result = generator.Run(commandline);
            return result;
        }

        private static void ReplaceCommandlineInExample(string docsDirectory, string assemblyDirectory, Generator generator, ExamplesGeneratorData generatorData)
        {
            var relativeRoot = Path.GetFullPath(Path.Combine(docsDirectory, ".."));
            var commandline = BuildExamplesGeneratorCommandline(generatorData, docsDirectory, assemblyDirectory, relativeRoot);
            var examplesCommandLine = $"dry-gen {string.Join(' ', commandline)}";
            var existingRepresentation = generator.ReadExistingRepresentationFromOutputFileAndValidateReplaceToken(generatorData.Verb, GetOutputFile(docsDirectory, generatorData), generatorData.ReplaceToken.AsCommandLineReplaceToken());
            var generatedRepresentation = existingRepresentation.Replace(generatorData.ReplaceToken.AsCommandLineReplaceToken(), examplesCommandLine);
            File.WriteAllText(GetOutputFile(docsDirectory, generatorData), generatedRepresentation);
        }

        private static string GetOutputFile(string docsDirectory, ExamplesGeneratorData generatorData)
        {
            return Path.Combine(docsDirectory, "examples", generatorData.OutputFile);
        }

        private static void GenerateVerbsMarkdown(string docsDirectory)
        {
            var verbs = typeof(Generator).Assembly.GetTypes().Where(x => x.HasVerbAttribute()).Select(x => x.GetVerb());
            foreach (var verb in verbs.OrderBy(x => x))
            {
                var verbMarkdownPath = Path.Combine(docsDirectory, "verbs", $"{verb}.md");
                Console.WriteLine($"Generating verb markdown for '{verb}' to \"{verbMarkdownPath}\"");
                using var verbMarkdownWriter = new StreamWriter(verbMarkdownPath);
                VerbMarkdowGenerator.Generate(verb, verbMarkdownWriter);
            }
        }

        private static void GenerateVerbsMenu(string docsDirectory)
        {
            var verbMenuPath = Path.Combine(docsDirectory, "_data", "verbs_menu.yml");
            Console.WriteLine($"Generating verbs menu to \"{verbMenuPath}\"");
            using var verbMenuWriter = new StreamWriter(verbMenuPath);
            VerbMenuGenerator.Generate(verbMenuWriter);
        }

        private static void GenerateExamplesMenu(string docsDirectory)
        {
            var examplesTemplateDirectory = Path.Combine(docsDirectory, "_templates", "examples");
            var examplesMenuPath = Path.Combine(docsDirectory, "_data", "examples_menu.yml");
            Console.WriteLine($"Generating examples menu to \"{examplesMenuPath}\"");
            using var examplesMenuWriter = new StreamWriter(examplesMenuPath);
            ExamplesMenuGenerator.Generate(examplesMenuWriter, examplesTemplateDirectory);
        }

        private static void CopyExamplesTemplates(string docsDirectory)
        {
            var examplesTemplatesDirectory = Path.Combine(docsDirectory, "_templates", "examples");
            var examplesDirectory = Path.Combine(docsDirectory, "examples");
            Console.WriteLine($"Copying examples template files from \"{examplesTemplatesDirectory}\" to \"{examplesDirectory}\"");
            foreach (var exampleTemplateFile in Directory.GetFiles(examplesTemplatesDirectory))
            {
                File.Copy(exampleTemplateFile, Path.Combine(examplesDirectory, Path.GetFileName(exampleTemplateFile)), true);
            }
        }

        private static IEnumerable<ExamplesGeneratorData> BuildExamplesGeneratorData()
        {
            return new[] {
                new ExamplesGeneratorData {
                    Verb = Constants.MermaidClassDiagramFromCsharp.Verb,
                    InputFile = "DryGen.MermaidFromCSharp.dll",
                    OutputFile = "Mermaid-Class-diagrams.md",
                    ReplaceToken = "class-diagram-one",
                },
            };
        }

        private static string[] BuildExamplesGeneratorCommandline(ExamplesGeneratorData generatorData, string docsDirectory, string assemblyDirectory, string? relativeRoot)
        {
            var inputFile = Path.Combine(assemblyDirectory, generatorData.InputFile); 
            if (!string.IsNullOrEmpty(relativeRoot))
            {
                inputFile = Path.GetRelativePath(relativeRoot, inputFile);
            }
            var outputFile = GetOutputFile(docsDirectory, generatorData);
            if (!string.IsNullOrEmpty(relativeRoot))
            {
                outputFile = Path.GetRelativePath(relativeRoot, outputFile);
            }
            var result = new List<string> {
                generatorData.Verb,
                $"--{Constants.InputFileOption}",
                inputFile,
                $"--{Constants.OutputFileOption}",
                outputFile,
                $"--{Constants.ReplaceTokenInOutputFile}",
                generatorData.ReplaceToken.AsGeneratedRepresentationReplaceToken(),
            };
            return result.ToArray();
        }
    }
}
