using DryGen.MermaidFromCSharp;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp.ClassDiagram
{
    public class ClassDiagramClass : NamedType
    {
        private readonly List<ClassDiagramAttribute> attributes;
        private readonly List<ClassDiagramMethod> methods;
        private readonly List<ClassDiagramRelationship> relationships;

        public ClassDiagramClass(NamedType other) : base(other.Name, other.Type)
        {
            attributes = new List<ClassDiagramAttribute>();
            methods = new List<ClassDiagramMethod>();
            relationships = new List<ClassDiagramRelationship>();
        }

        public IReadOnlyList<ClassDiagramAttribute> Attributes => attributes;
        public IReadOnlyList<ClassDiagramMethod> Methods => methods;
        public IReadOnlyList<ClassDiagramRelationship> Relationships => relationships;

        public void AddAttribute(ClassDiagramAttribute attribute)
        {
            attributes.Add(attribute);
        }

        public void AddMethod(ClassDiagramMethod method)
        {
            methods.Add(method);
        }

        public void AddRelationship(
            ClassDiagramRelationshipCardinality fromCardinality,
            ClassDiagramRelationshipType relationshipType,
            ClassDiagramRelationshipCardinality toCardinality,
            ClassDiagramClass to, string label, string propertyName)
        {
            var isBidirectional = false;
            if (relationshipType == ClassDiagramRelationshipType.Dependency)
            {
                if (Relationships.Any(x => x.RelationsshipType == ClassDiagramRelationshipType.Dependency && x.To == to))
                {
                    // Avoid duplicate dependency
                    return;
                }
            }
            else
            {
                // Remove any dependencies created from contrutor parameters
                relationships.RemoveAll(r => r.RelationsshipType == ClassDiagramRelationshipType.Dependency && r.To == to);
                if (relationshipType == ClassDiagramRelationshipType.Association)
                {
                    // Switch from composision to aggregation for all bidirectional collection references 
                    foreach (var relationship in to.Relationships.Where(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Composition))
                    {
                        relationship.RelationsshipType = ClassDiagramRelationshipType.Aggregation;
                        relationship.IsBidirectional = true;
                        relationship.FromCardinality = toCardinality;
                        if (!string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(relationship.Label))
                        {
                            relationship.Label = label;
                        }
                    }
                    // Skip the relation if we already has a bidirectional collection reference 
                    if (to.Relationships.Any(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Aggregation))
                    {
                        return;
                    }
                    var otherAssociation = to.Relationships.FirstOrDefault(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Association && !x.IsBidirectional);
                    if (otherAssociation != null)
                    {
                        otherAssociation.IsBidirectional = true;
                        otherAssociation.FromCardinality = toCardinality;
                        return;
                    }
                }
                else if (relationshipType == ClassDiagramRelationshipType.Composition)
                {
                    // Switch from composision to aggregation if this is a bidirectional collection references 
                    var otherAssociation = to.Relationships.FirstOrDefault(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Association && !x.IsBidirectional);
                    if (otherAssociation != null)
                    {
                        relationshipType = ClassDiagramRelationshipType.Aggregation;
                        isBidirectional = true;
                        fromCardinality = otherAssociation.ToCardinality;
                        if (string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(otherAssociation.Label))
                        {
                            label = otherAssociation.Label;
                        }
                    }
                }
            }
            var relationShip = new ClassDiagramRelationship(fromCardinality, relationshipType, toCardinality, to, label, propertyName, isBidirectional);
            relationships.Add(relationShip);
        }

        public void RemoveBidirectionalRelationshipDuplicates()
        {
            relationships.RemoveAll(r =>
                    r.RelationsshipType == ClassDiagramRelationshipType.Association &&
                    !r.IsBidirectional &&
                    r.To.Relationships.Any(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Aggregation));
        }
    }
}
