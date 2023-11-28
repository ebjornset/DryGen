namespace DryGen.MermaidFromCSharp.ErDiagram;

public interface IMermaidErDiagramFromCSharpOptions : IMermaidDiagramOptions
{
    ErDiagramAttributeTypeExclusion? AttributeTypeExclusion { get; }
    ErDiagramAttributeDetailExclusions AttributeDetailExclusions { get; }
    ErDiagramRelationshipTypeExclusion? RelationshipTypeExclusion { get; }
    IErDiagramStructureBuilder StructureBuilder { get; }
}