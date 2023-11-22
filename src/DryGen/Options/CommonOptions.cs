using CommandLine;
using YamlDotNet.Serialization;

namespace DryGen.Options;

public abstract class CommonOptions : BaseOptions
{
    [YamlMember(Alias = Constants.OutputFileOption, ApplyNamingConventions = false)]
    [Option('o', Constants.OutputFileOption, HelpText = "Write the generated representation to this file.")]
    public string? OutputFile { get; set; }

    [YamlMember(Alias = Constants.ReplaceTokenInOutputFile, ApplyNamingConventions = false)]
    [Option(Constants.ReplaceTokenInOutputFile, HelpText = "Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.")]
    public string? ReplaceTokenInOutputFile { get; set; }
}
