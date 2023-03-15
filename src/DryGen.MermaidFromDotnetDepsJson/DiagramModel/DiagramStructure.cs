using DryGen.MermaidFromDotnetDepsJson.DeptsModel;

namespace DryGen.MermaidFromDotnetDepsJson.DiagramModel;

internal class DiagramStructure : DiagramStructureElement
{
    internal DiagramStructure(Dependency mainAssembly)
    {
        MainAssembly = mainAssembly;
    }
    public Dependency MainAssembly { get; }
}
