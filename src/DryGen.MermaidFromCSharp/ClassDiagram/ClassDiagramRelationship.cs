namespace DryGen.MermaidFromCSharp.ClassDiagram;

public class ClassDiagramRelationship
{
    public ClassDiagramRelationship(
        ClassDiagramRelationshipCardinality fromCardinality,
        ClassDiagramRelationshipType relationsshipType,
        ClassDiagramRelationshipCardinality toCardinality,
        ClassDiagramClass to,
        string label,
        string propertyName,
        bool isBidirectional)
    {
        FromCardinality = fromCardinality;
        To = to;
        RelationsshipType = relationsshipType;
        ToCardinality = toCardinality;
        Label = label;
        PropertyName = propertyName;
        IsBidirectional = isBidirectional;
    }

    public ClassDiagramRelationshipCardinality FromCardinality { get; set; }
    public ClassDiagramClass To { get; }
    public ClassDiagramRelationshipType RelationsshipType { get; set; }
    public ClassDiagramRelationshipCardinality ToCardinality { get; }
    public string Label { get; set; }
    public string PropertyName { get; }
    public bool IsBidirectional { get; set; }
}