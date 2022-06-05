using System.Collections.Generic;

namespace DryGen.Docs
{
    public class ExamplesGeneratorData
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string ReplaceToken { get; set; }
        public string Verb { get; set; }
        public IEnumerable<string> AdditionalOptions { get; set; }
    }
}
