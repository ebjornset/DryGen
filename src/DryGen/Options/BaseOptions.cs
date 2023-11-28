using CommandLine;

namespace DryGen.Options;

public abstract class BaseOptions
{
    [Option('f', Constants.OptionsFileOption, HelpText = "Read options from this file.")]
    public string? OptionsFile { get; set; }

    [Option("include-exception-stacktrace", Hidden = true)]
    public bool IncludeExceptionStackTrace { get; set; }
}