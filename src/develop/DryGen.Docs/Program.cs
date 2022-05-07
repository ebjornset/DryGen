﻿using CommandLine;
using DryGen;
using DryGen.Docs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

return Parser.Default.ParseArguments<Options>(args)
  .MapResult(
    options => RunAndReturnExitCode(options),
    _ => 1);

static int RunAndReturnExitCode(Options options)
{
    var outputFolder = Path.GetFullPath(options.Output);
    string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!Directory.Exists(outputFolder))
    {
        throw new ArgumentException($"output folder '{outputFolder}' does not exist!");
    }
    var generator = new Generator(Console.Out, Console.Error);
    var result = 0;
    foreach (var generatorData in BuildGeneratorData())
    {
        var commandLine = BuildCommandline(generatorData, outputFolder, assemblyFolder);
        Console.WriteLine($"Generating: {string.Join(' ', commandLine)}");
        result = generator.Run(commandLine);
        if (result > 0)
        {
            break;
        }
    }
    return result;
}

static IEnumerable<GeneratorData> BuildGeneratorData()
{
    return new[] {
        new GeneratorData {
            Verb = Constants.MermaidClassDiagramFromCsharp.Verb,
            InputFile = "DryGen.MermaidFromCSharp.dll",
            OptionsFile = "DryGen.Mermaid.ClassDiagram.From.CSharp.For.ClassDiagram.yaml",
        },
        new GeneratorData {
            Verb = Constants.MermaidClassDiagramFromCsharp.Verb,
            InputFile = "DryGen.MermaidFromCSharp.dll",
            OptionsFile = "DryGen.Mermaid.ClassDiagram.From.CSharp.For.CommonClasses.yaml",
        },
        new GeneratorData {
            Verb = Constants.MermaidClassDiagramFromCsharp.Verb,
            InputFile = "DryGen.MermaidFromCSharp.dll",
            OptionsFile = "DryGen.Mermaid.ClassDiagram.From.CSharp.For.ErDiagram.yaml",
        },
        new GeneratorData {
            Verb = Constants.MermaidClassDiagramFromCsharp.Verb,
            InputFile = "DryGen.MermaidFromCSharp.EfCore.dll",
            OptionsFile = "DryGen.Mermaid.ClassDiagram.From.CSharp.For.EfCore.yaml",
        },
    };
}

static string[] BuildCommandline(GeneratorData generatorData, string outputFolder, string assemblyFolder)
{
    return new[]
    {
        generatorData.Verb,
        $"--{Constants.InputFileOption}",
        Path.Combine(assemblyFolder, generatorData.InputFile),
        $"--{Constants.OptionsFileOption}",
        Path.Combine(assemblyFolder, generatorData.OptionsFile),
        $"--{Constants.OutputFileOption}",
        Path.Combine(outputFolder, Path.ChangeExtension(generatorData.OptionsFile, "mmd")),
    };
}

namespace DryGen.Docs
{
    class GeneratorData
    {
        public string InputFile { get; set; }
        public string OptionsFile { get; set; }
        public string Verb { get; set; }
    }

    class Options
    {
        [Option('o', "output", Required = true, HelpText = "Set output folder.")]
        public string Output { get; set; }
    }
}
