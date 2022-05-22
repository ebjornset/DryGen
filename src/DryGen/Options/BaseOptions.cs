using CommandLine;
using DryGen.Core;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    public abstract class BaseOptions: IInputFileOptions
    {
        [YamlMember(Alias = Constants.InputFileOption, ApplyNamingConventions = false)]
        [Option('i', Constants.InputFileOption, HelpText = "Full path to the input file to generate a diagram for.")]
        public string? InputFile { get; set; }

        [YamlMember(Alias = "output-file", ApplyNamingConventions = false)]
        [Option('o', "output-file", HelpText = "Write the resulting diagram to this file.")]
        public string? OutputFile { get; set; }

        [YamlMember(Alias = "options-file", ApplyNamingConventions = false)]
        [Option('f', "options-file", HelpText = "Read options from this file.")]
        public string? OptionsFile { get; set; }
    }
}