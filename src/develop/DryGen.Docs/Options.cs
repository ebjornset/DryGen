using CommandLine;

namespace DryGen.Docs
{
    public class Options
    {
        [Option("docs-directory", Required = true, HelpText = "Set the docs directory where stuff will be generated.")]
        public string DocsDirectory { get; set; }
    }
}
