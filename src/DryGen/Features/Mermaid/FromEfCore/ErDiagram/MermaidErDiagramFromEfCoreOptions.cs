using CommandLine;
using DryGen.MermaidFromEfCore;
using DryGen.Options;

namespace DryGen.Features.Mermaid.FromEfCore.ErDiagram;

[Verb(Constants.MermaidErDiagramFromEfCore.Verb, HelpText = "Generate a Mermaid Entity Relationship diagram from a C# assembly using Entity Framework Core.")]
public class MermaidErDiagramFromEfCoreOptions : MermaidErDiagramFromCsharpBaseOptions
{
    public MermaidErDiagramFromEfCoreOptions() : base(new ErDiagramStructureBuilderByEfCore()) { }
}
