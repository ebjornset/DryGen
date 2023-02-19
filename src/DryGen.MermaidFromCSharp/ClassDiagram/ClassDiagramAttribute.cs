namespace DryGen.MermaidFromCSharp.ClassDiagram;

public class ClassDiagramAttribute
{
    public ClassDiagramAttribute(string attributeType, string attributeName, string visibility, bool isStatic)
    {
        AttributeType = attributeType;
        AttributeName = attributeName;
        Visibility = visibility;
        IsStatic = isStatic;
    }

    public string AttributeType { get; }
    public string AttributeName { get; }
    public string Visibility { get; }
    public bool IsStatic { get; }
}
