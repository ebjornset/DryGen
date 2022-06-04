using CommandLine;
using DryGen.Core;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    public abstract class BaseOptions: IInputFileOptions
    {
        [YamlMember(Alias = Constants.InputFileOption, ApplyNamingConventions = false)]
        [Option('i', Constants.InputFileOption, HelpText = "Full path to the input file to generate a new representation for.")]
        public string? InputFile { get; set; }

        [YamlMember(Alias = "output-file", ApplyNamingConventions = false)]
        [Option('o', "output-file", HelpText = "Write the generated representation to this file.")]
        public string? OutputFile { get; set; }

        [YamlMember(Alias = "replace-token-in-output-file", ApplyNamingConventions = false)]
        [Option("replace-token-in-output-file", HelpText = "Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.")]
        public string? RplaceTokenInOutputFile { get; set; }

        [Option('f', "options-file", HelpText = "Read options from this file.")]
        public string? OptionsFile { get; set; }
    }
}