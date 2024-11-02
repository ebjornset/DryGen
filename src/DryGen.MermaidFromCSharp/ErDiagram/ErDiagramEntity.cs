using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp.ErDiagram;

public class ErDiagramEntity : NamedType
{
    private readonly List<ErDiagramAttribute> attributes;
    private readonly List<ErDiagramRelationship> relationships;

    public IReadOnlyList<ErDiagramRelationship> Relationships => relationships;
    public IReadOnlyList<ErDiagramAttribute> Attributes => attributes;

    public ErDiagramEntity(NamedType other) : this(other.Name, other.Type)
    {
    }

    public ErDiagramEntity(string name, Type type) : base(name, type)
    {
        attributes = new List<ErDiagramAttribute>();
        relationships = new List<ErDiagramRelationship>();
    }

    public IReadOnlyList<ErDiagramAttribute> GetAttributes() => Attributes.OrderBy(x => x.IsPrimaryKey ? 0 : 1).ThenBy(x => x.IsAlternateKey ? 0 : 1)
        .ThenBy(x => x.IsForeignKey ? 0 : 1).ThenBy(x => x.IsNullable ? 1 : 0).ThenBy(x => x.AttributeName).ToArray();

    public IReadOnlyList<ErDiagramRelationship> GetRelationships() => Relationships.OrderBy(x => x.PropertyName).ToArray();

    public void AddAttribute(ErDiagramAttribute attribute)
    {
        attributes.Add(attribute);
    }

    public void AddRelationship(ErDiagramEntity to, ErDiagramRelationshipCardinality fromCardinality, ErDiagramRelationshipCardinality toCardinality, string label, string propertyName, bool isIdentifying = false)
    {
        var relationship = new ErDiagramRelationship(to, fromCardinality, toCardinality, label, propertyName, isIdentifying);
        relationships.Add(relationship);
    }

    public void RemoveRelationship(ErDiagramRelationship relationship)
    {
        relationships.Remove(relationship);
    }

    public void RemoveBidirectionalRelationshipDuplicates()
    {
        relationships.RemoveAll(r =>
            r.ToCardinality.IsNotMany()
            && string.IsNullOrEmpty(r.Label)
            && (GetRelationships().Any(x => x != r && x.To == r.To)
                || r.To.GetRelationships().Any(x => x.To == this)));
    }

public void MergeTwoOneToManyIntoOneMayToMany()
    {
        bool wasModified;
        do
        {
            // Must use an outer loop since a self referencing many to many will modify the relationships collection and the enumeration will crash
            wasModified = false;
            foreach (var relationship in relationships.Where(x => x.FromCardinality == ErDiagramRelationshipCardinality.ExactlyOne && x.ToCardinality == ErDiagramRelationshipCardinality.ZeroOrMore && ! x.IsIdenifying)) {
                var backRelationship = relationship.To.Relationships.FirstOrDefault(x => x.To == this && x.FromCardinality == ErDiagramRelationshipCardinality.ExactlyOne && x.ToCardinality == ErDiagramRelationshipCardinality.ZeroOrMore && ! x.IsIdenifying && x != relationship);
                if (backRelationship == null) {
                    continue;
                }
                relationship.MergeAsManyToManyWith(backRelationship);
                wasModified = true;
                break;
            }
        } while (wasModified);
    }

    protected override bool IsRelatedTo(IDiagramType type)
    {
        var result = relationships.Exists(x => x.To.Type == type.Type);
        if (!result && type is ErDiagramEntity to)
        {
            result = to.Relationships.Any(x => x.To.Type == Type);
        }
        return result;
    }
}