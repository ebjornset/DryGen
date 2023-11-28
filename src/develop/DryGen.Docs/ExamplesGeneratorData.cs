using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.Docs;

[ExcludeFromCodeCoverage]
public class ExamplesGeneratorData
{
    public string InputFile { get; set; }
    public string OutputFile { get; set; }
    public string ReplaceToken { get; set; }
    public string Verb { get; set; }
    public IEnumerable<string> AdditionalOptions { get; set; }
}