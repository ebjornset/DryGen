using CommandLine;

namespace DryGen
{

    [Verb(Constants.MermaidErDiagramFromCsharp.Verb, HelpText = "Generate a Mermaid Entity Relationship diagram from a C# assembly using reflection.")]
    public class MermaidErDiagramFromCSharpOptions : MermaidErDiagramFromCSharpBaseOptions
    {
        public MermaidErDiagramFromCSharpOptions() : base(ErStructureBuilderType.Reflection) {}
    }
}
