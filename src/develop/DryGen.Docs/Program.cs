using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DryGen.Docs
{
    [ExcludeFromCodeCoverage] // We run this from nuke docs, so we are not to worried about the code coverage at the moment
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
            CopyExamplesTemplates(rootDirectory);
            var result = 0;
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var generator = new Generator(Console.Out, Console.Error);
            foreach (var generatorData in BuildExamplesGeneratorData())
            {
                result = ReplaceRepresentationInExample(rootDirectory, assemblyDirectory, generator, generatorData);
                if (result > 0)
                {
                    break;
                }
                ReplaceCommandlineInExample(rootDirectory, assemblyDirectory, generator, generatorData);
            }
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
            return Path.Combine(rootDirectory, "docs", "examples", generatorData.OutputFile.ToLowerInvariant());
        }

        private static void GenerateVerbsMarkdown(string rootDirectory)
        {
            var verbs = typeof(Generator).Assembly.GetTypes().Where(x => x.HasVerbAttribute()).Select(x => x.GetVerb());
            foreach (var verb in verbs.OrderBy(x => x))
            {
                var verbMarkdownPath = Path.Combine(rootDirectory, "docs", "verbs", $"{verb}.md");
                Console.WriteLine($"Generating verb markdown for '{verb}' to \"{verbMarkdownPath}\"");
                using var verbMarkdownWriter = new StreamWriter(verbMarkdownPath);
                VerbMarkdowGenerator.Generate(verb, verbMarkdownWriter);
            }
        }

        private static void GenerateVerbsMenu(string rootDirectory)
        {
            var verbMenuPath = Path.Combine(rootDirectory, "docs", "_data", "verbs_menu.yml");
            Console.WriteLine($"Generating verbs menu to \"{verbMenuPath}\"");
            using var verbMenuWriter = new StreamWriter(verbMenuPath);
            VerbMenuGenerator.Generate(verbMenuWriter);
        }

        private static void GenerateExamplesMenu(string rootDirectory)
        {
            var examplesTemplateDirectory = Path.Combine(rootDirectory, "docs", "_templates", "examples");
            var examplesMenuPath = Path.Combine(rootDirectory, "docs", "_data", "examples_menu.yml");
            Console.WriteLine($"Generating examples menu to \"{examplesMenuPath}\"");
            using var examplesMenuWriter = new StreamWriter(examplesMenuPath);
            ExamplesMenuGenerator.Generate(examplesMenuWriter, examplesTemplateDirectory);
        }

        private static void CopyExamplesTemplates(string rootDirectory)
        {
            var examplesTemplatesDirectory = Path.Combine(rootDirectory, "docs", "_templates", "examples");
            var examplesDirectory = Path.Combine(rootDirectory, "docs", "examples");
            Console.WriteLine($"Copying examples template files from \"{examplesTemplatesDirectory}\" to \"{examplesDirectory}\"");
            foreach (var exampleTemplateFile in Directory.GetFiles(examplesTemplatesDirectory))
            {
                File.Copy(exampleTemplateFile, Path.Combine(examplesDirectory, Path.GetFileName(exampleTemplateFile.ToLowerInvariant())), true);
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
            return result.ToArray();
        }
    }
}
