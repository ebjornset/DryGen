using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DryGen.Core;

namespace DryGen.MermaidFromCSharp.ClassDiagram;

public static class ClassDiagramExtensions
{
    public static string GetRelationshipPattern(this ClassDiagramRelationship relationship)
    {
        var relationshipType = relationship.RelationsshipType;
        var relationshipLine = relationshipType.GetRelationshipLine();
        var relationshipStartArrow = relationshipType.GetRelationshipStartArrow(relationship.IsBidirectional);
        var relationshipEndArrow = relationshipType.GetRelationshipEndArrow();
        var fromCardinality = relationship.FromCardinality.GetCardinalityValue();
        var toCardinality = relationship.ToCardinality.GetCardinalityValue();
        return relationship.RelationsshipType switch
        {
            ClassDiagramRelationshipType.Dependency or
            ClassDiagramRelationshipType.Realization or
            ClassDiagramRelationshipType.Inheritance
            => $"{fromCardinality}{relationshipStartArrow}{relationshipLine}{relationshipEndArrow}{toCardinality}",
            ClassDiagramRelationshipType.Association or
            ClassDiagramRelationshipType.Composition or
            ClassDiagramRelationshipType.Aggregation
            => $"{fromCardinality}{relationshipStartArrow}{relationshipLine}{relationshipEndArrow}{toCardinality}",
            _ => throw new ArgumentException($"Unexpected {nameof(relationship.RelationsshipType)}: {relationship.RelationsshipType}"),
        };
    }

    public static string GetCardinalityValue(this ClassDiagramRelationshipCardinality cardinality)
    {
        return cardinality switch
        {
            ClassDiagramRelationshipCardinality.ZeroOrOne => " \"0..1\" ",
            ClassDiagramRelationshipCardinality.ExactlyOne => " \"1\" ",
            ClassDiagramRelationshipCardinality.ZeroOrMore => " \"*\" ",
            ClassDiagramRelationshipCardinality.OneOrMore => " \"1..*\" ",
            ClassDiagramRelationshipCardinality.Unspecified => " ",
            _ => throw new ArgumentException(cardinality.ToString(), nameof(cardinality)),
        };
    }

    public static string GetRelationshipLine(this ClassDiagramRelationshipType relationship)
    {
        return relationship switch
        {
            ClassDiagramRelationshipType.Inheritance or
            ClassDiagramRelationshipType.Composition or
            ClassDiagramRelationshipType.Aggregation or
            ClassDiagramRelationshipType.Association
                => "--",
            ClassDiagramRelationshipType.Dependency or
            ClassDiagramRelationshipType.Realization
                => "..",
            _ => throw new ArgumentException(relationship.ToString(), nameof(relationship)),
        };
    }

    public static string GetRelationshipStartArrow(this ClassDiagramRelationshipType relationship, bool isBidirectional)
    {
        return relationship switch
        {
            ClassDiagramRelationshipType.Association
                => isBidirectional ? "<" : string.Empty,
            ClassDiagramRelationshipType.Dependency or
            ClassDiagramRelationshipType.Realization or
            ClassDiagramRelationshipType.Inheritance
                => string.Empty,
            ClassDiagramRelationshipType.Composition
                => "*",
            ClassDiagramRelationshipType.Aggregation
                => "o",
            _ => throw new ArgumentException(relationship.ToString(), nameof(relationship)),
        };
    }

    public static string GetRelationshipEndArrow(this ClassDiagramRelationshipType relationship)
    {
        return relationship switch
        {
            ClassDiagramRelationshipType.Composition or
            ClassDiagramRelationshipType.Aggregation
                => string.Empty,
            ClassDiagramRelationshipType.Association or
            ClassDiagramRelationshipType.Dependency
                => ">",
            ClassDiagramRelationshipType.Inheritance or
            ClassDiagramRelationshipType.Realization
                => "|>",
            _ => throw new ArgumentException(relationship.ToString(), nameof(relationship)),
        };
    }

    public static ClassDiagramRelationshipCardinality GetAssociationToCardinality(this PropertyInfo property, bool isNullable)
    {
        if (!isNullable && property.IsRequiredProperty())
        {
            return ClassDiagramRelationshipCardinality.ExactlyOne;
        }
        return ClassDiagramRelationshipCardinality.ZeroOrOne;
    }

    public static string GetRelationshipLabel(this ClassDiagramRelationship relationship)
    {
        return string.IsNullOrWhiteSpace(relationship.Label) ? string.Empty : $" : {relationship.Label}";
    }

    public static bool IsExtensionType(this Type type)
    {

        return type.IsAbstract && type.IsSealed && !type.IsNested && type.CustomAttributes.Any(x => x.AttributeType == typeof(ExtensionAttribute));
    }

    public static bool IsExtensionMethod(this MethodInfo methodInfo)
    {
        return methodInfo.IsStatic && methodInfo.GetParameters().Any() && methodInfo.CustomAttributes.Any(x => x.AttributeType == typeof(ExtensionAttribute));
    }
}
