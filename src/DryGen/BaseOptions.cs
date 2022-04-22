using CommandLine;
using YamlDotNet.Serialization;

namespace DryGen
{
    public abstract class BaseOptions
    {
        [YamlMember(Alias = Constants.InputFileOption, ApplyNamingConventions = false)]
        [Option('i', Constants.InputFileOption, HelpText = "Full path to the input file to generate a diagram for.")]
        public string? AssemblyFile { get; set; }

        [YamlMember(Alias = "output-file", ApplyNamingConventions = false)]
        [Option('o', "output-file", HelpText = "Write the resulting diagram to this file.")]
        public string? OutputFile { get; set; }

        [YamlMember(Alias = "options-file", ApplyNamingConventions = false)]
        [Option('f', "options-file", HelpText = "Read options from this file.")]
        public string? OptionsFile { get; set; }

        [YamlMember(Alias = "name-replace-from", ApplyNamingConventions = false)]
        [Option("name-replace-from", HelpText = "A string to replace in all class/entity names.")]
        public string? NameReplaceFrom { get; set; }

        [YamlMember(Alias = "name-replace-to", ApplyNamingConventions = false)]
        [Option("name-replace-to", HelpText = "The string to replace with in all class/entity names.")]
        public string? NameReplaceTo { get; set; }
    }
}