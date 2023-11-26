using CommandLine;
using DryGen.MermaidFromCSharp.ErDiagram;
using DryGen.Options;

namespace DryGen.Features.Mermaid.FromCsharp.ErDiagram;

[Verb(Constants.MermaidErDiagramFromCsharp.Verb, HelpText = "Generate a Mermaid Entity Relationship diagram from a C# assembly using reflection.")]
public class MermaidErDiagramFromCsharpOptions : MermaidErDiagramFromCsharpBaseOptions
{
    public MermaidErDiagramFromCsharpOptions() : base(new ErDiagramStructureBuilderByReflection()) { }
}
