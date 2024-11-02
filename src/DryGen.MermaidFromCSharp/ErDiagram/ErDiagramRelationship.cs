namespace DryGen.MermaidFromCSharp.ErDiagram;

public class ErDiagramRelationship
{
    public ErDiagramRelationship(
        ErDiagramEntity to,
        ErDiagramRelationshipCardinality fromCardinality,
        ErDiagramRelationshipCardinality toCardinality,
        string label,
        string propertyName,
        bool isIdenifying)
    {
        To = to;
        FromCardinality = fromCardinality;
        ToCardinality = toCardinality;
        Label = label;
        PropertyName = propertyName;
        IsIdenifying = isIdenifying;
    }

    public ErDiagramEntity To { get; }
    public ErDiagramRelationshipCardinality FromCardinality { get; private set; }
    public ErDiagramRelationshipCardinality ToCardinality { get; }
    public string Label { get; }
    internal string PropertyName { get; }
    public bool IsIdenifying { get; }

    public void MergeAsManyToManyWith(ErDiagramRelationship backRelationship) {
        FromCardinality = ErDiagramRelationshipCardinality.ZeroOrMore;
        To.RemoveRelationship(backRelationship);
    }
}