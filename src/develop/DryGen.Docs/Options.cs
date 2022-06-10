using CommandLine;

namespace DryGen.Docs
{
    public class Options
    {
        [Option("root-directory", Required = true, HelpText = "Set the root directory, assuming this is the parent directory of the docs directory where stuff will be generated.")]
        public string RootDirectory { get; set; }
    }
}
