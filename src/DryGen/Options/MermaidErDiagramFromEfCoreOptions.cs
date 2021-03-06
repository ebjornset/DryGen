using CommandLine;
using DryGen.MermaidFromEfCore;

namespace DryGen.Options
{
    [Verb(Constants.MermaidErDiagramFromEfCore.Verb, HelpText = "Generate a Mermaid Entity Relationship diagram from a C# assembly using Entity Framework Core.")]
    public class MermaidErDiagramFromEfCoreOptions : MermaidErDiagramFromCSharpBaseOptions
    {
        public MermaidErDiagramFromEfCoreOptions() : base(new ErDiagramStructureBuilderByEfCore()) { }
    }
}
