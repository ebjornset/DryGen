namespace DryGen.MermaidFromCSharp.ClassDiagram
{
    public interface IMermaidClassDiagramFromCSharpOptions : IMermaidDiagramOptions
    {
        ClassDiagramAttributeLevel? AttributeLevel { get; }
        ClassDiagramMethodLevel? MethodLevel { get; }
        ClassDiagramDirection? Direction { get; }
        bool? ExcludeStaticAttributes { get; }
        bool? ExcludeStaticMethods { get; }
        bool? ExcludeMethodParams { get; }
    }
}
