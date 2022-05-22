using CommandLine;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    [Verb(Constants.OptionsFromCommandline.Verb, HelpText = "Generate dry-gen options for a verb from the command line.")]
    public class OptionsFromCommandlineOptions : BaseOptions
    {
        [YamlMember(Alias = "verb", ApplyNamingConventions = false)]
        [Option("verb", HelpText = "The dryg-gen verb to generate options for.")]
        public string? Verb { get; set; }
    }
}
