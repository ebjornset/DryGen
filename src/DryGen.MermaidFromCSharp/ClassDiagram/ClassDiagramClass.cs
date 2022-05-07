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
                if (IsDuplicateDependency(to))
                {
                    return;
                }
            }
            else
            {
                RemoveAnyDependenciesCreatedFromContrutorParameters(to);
                if (relationshipType == ClassDiagramRelationshipType.Association)
                {
                    SwitchFromComposisionToAggregationForAllBidirectionalCollectionReferences(toCardinality, to, label);
                    // Skip the relation if we already has a bidirectional collection reference 
                    if (HasBidirectionalRelationship(to))
                    {
                        return;
                    }
                    var otherAssociation = FindNonBidirectionalAssociationToThisThatMatchesThisAssociation(to);
                    if (otherAssociation != null)
                    {
                        otherAssociation.IsBidirectional = true;
                        otherAssociation.FromCardinality = toCardinality;
                        return;
                    }
                }
                else if (relationshipType == ClassDiagramRelationshipType.Composition)
                {
                    SwitchFroComposisionToAggregationIfThisIsABidirectionalCollectionReferences(ref fromCardinality, ref relationshipType, to, ref label, ref isBidirectional);
                }
            }
            var relationShip = new ClassDiagramRelationship(fromCardinality, relationshipType, toCardinality, to, label, propertyName, isBidirectional);
            relationships.Add(relationShip);

            void SwitchFromComposisionToAggregationForAllBidirectionalCollectionReferences(ClassDiagramRelationshipCardinality toCardinality, ClassDiagramClass to, string label)
            {
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
            }

            bool HasBidirectionalRelationship(ClassDiagramClass to)
            {
                return to.Relationships.Any(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Aggregation);
            }

            void SwitchFroComposisionToAggregationIfThisIsABidirectionalCollectionReferences(ref ClassDiagramRelationshipCardinality fromCardinality, ref ClassDiagramRelationshipType relationshipType, ClassDiagramClass to, ref string label, ref bool isBidirectional)
            {
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

            void RemoveAnyDependenciesCreatedFromContrutorParameters(ClassDiagramClass to)
            {
                relationships.RemoveAll(r => r.RelationsshipType == ClassDiagramRelationshipType.Dependency && r.To == to);
            }

            bool IsDuplicateDependency(ClassDiagramClass to)
            {
                return Relationships.Any(x => x.RelationsshipType == ClassDiagramRelationshipType.Dependency && x.To == to);
            }

            ClassDiagramRelationship FindNonBidirectionalAssociationToThisThatMatchesThisAssociation(ClassDiagramClass to)
            {
                return to.Relationships.FirstOrDefault(x => x.To == this && x.RelationsshipType == ClassDiagramRelationshipType.Association && !x.IsBidirectional);
            }
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
