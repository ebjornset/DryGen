using DryGen.MermaidFromDotnetDepsJson.DeptsModel;

namespace DryGen.MermaidFromDotnetDepsJson.DiagramModel;

internal class Component : DiagramStructureElement
{
    internal Component(Dependency dependency)
    {
        Dependency = dependency;
    }

    internal Dependency Dependency { get; }
}