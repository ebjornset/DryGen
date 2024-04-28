using CommandLine;
using YamlDotNet.Serialization;

namespace DryGen.Options;

public abstract class BaseOptions
{
    [Option('f', Constants.OptionsFileOption, HelpText = "Read options from this file.")]
    public string? OptionsFile { get; set; }

    [YamlMember(Alias = Constants.OutputTemplate, ApplyNamingConventions = false)]
    [Option(Constants.OutputTemplate, HelpText = "Template text for controlling the final output. Use ${DryGenOutput} to include the generated representation in the result")]
    public string? OutputTemplate { get; set; }

    [Option("include-exception-stacktrace", Hidden = true)]
    public bool IncludeExceptionStackTrace { get; set; }
}