namespace DryGen.MermaidFromDotnetDepsJson.DiagramModel;

internal class ContainerBoundary : DiagramStructureElement
{
    internal ContainerBoundary(string name, bool dontSuppress)
    {
        Alias = name;
        DontSuppress = dontSuppress;
        Label = name;
    }

    public string Alias { get; }
    public bool DontSuppress { get; }
    public string Label { get; }
}