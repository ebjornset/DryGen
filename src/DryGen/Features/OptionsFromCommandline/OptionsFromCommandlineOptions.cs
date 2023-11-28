using CommandLine;
using DryGen.Options;
using YamlDotNet.Serialization;

namespace DryGen.Features.OptionsFromCommandline;

[Verb(Constants.OptionsFromCommandline.Verb, HelpText = "Generate dry-gen options for a verb from the command line.")]
public class OptionsFromCommandlineOptions : CommonOptions
{
    [YamlMember(Alias = "verb", ApplyNamingConventions = false)]
    [Option("verb", HelpText = "The dryg-gen verb to generate options for.")]
    public string? Verb { get; set; }
}