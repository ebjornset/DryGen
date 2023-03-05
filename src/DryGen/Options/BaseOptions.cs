using CommandLine;
using DryGen.Core;
using YamlDotNet.Serialization;

namespace DryGen.Options;

public abstract class BaseOptions : IInputFileOptions
{
    [YamlMember(Alias = Constants.InputFileOption, ApplyNamingConventions = false)]
    [Option('i', Constants.InputFileOption, HelpText = "Full path to the input file to generate a new representation for.")]
    public string? InputFile { get; set; }

    [YamlMember(Alias = Constants.OutputFileOption, ApplyNamingConventions = false)]
    [Option('o', Constants.OutputFileOption, HelpText = "Write the generated representation to this file.")]
    public string? OutputFile { get; set; }

    [YamlMember(Alias = Constants.ReplaceTokenInOutputFile, ApplyNamingConventions = false)]
    [Option(Constants.ReplaceTokenInOutputFile, HelpText = "Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.")]
    public string? ReplaceTokenInOutputFile { get; set; }

    [Option('f', Constants.OptionsFileOption, HelpText = "Read options from this file.")]
    public string? OptionsFile { get; set; }

    [Option("include-exception-stacktrace", Hidden = true)]
    public bool IncludeExceptionStackTrace { get; set; }
}