using CommandLine;
using DryGen.Core;
using YamlDotNet.Serialization;

namespace DryGen.Options;

public abstract class CommonInputFileOptions : CommonOptions, IInputFileOptions
{
    [YamlMember(Alias = Constants.InputFileOption, ApplyNamingConventions = false)]
    [Option('i', Constants.InputFileOption, HelpText = "Full path to the input file to generate a new representation for.")]
    public string? InputFile { get; set; }
}