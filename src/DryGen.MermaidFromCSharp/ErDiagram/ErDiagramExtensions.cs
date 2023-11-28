using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryGen.MermaidFromCSharp.ErDiagram;

public static class ErDiagramExtensions
{
    public static bool IsErDiagramRelationshipCollection(this Type type)
    {
        return collectionTypes.Contains(type);
    }

    public static bool IsErDiagramAttributePropertyType(this PropertyInfo property)
    {
        return property.CanRead
            && property.CanWrite
            && !property.PropertyType.IsArray
            && property.PropertyType.IsErDiagramAttributePropertyType();
    }

    public static bool IsErDiagramAttributePropertyType(this Type propertyType)
    {
        if (propertyType.IsPrimitive || Array.Exists(nonPrivitiveAttributeTypes, x => x == propertyType))
        {
            return true;
        }
        var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
        if (nullableUnderlyingType != null)
        {
            return nullableUnderlyingType.IsErDiagramAttributePropertyType();
        }
        return false;
    }

    public static string GetErDiagramAttributeTypeName(this PropertyInfo property)
    {
        return property.PropertyType.GetErDiagramAttributeTypeName();
    }

    public static string GetErDiagramAttributeTypeName(this Type type)
    {
        var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
        var propertyTypename = (nullableUnderlyingType ?? type).Name;
        var result = GetPropertyTypeName(propertyTypename);
        return result;

        static string GetPropertyTypeName(string propertyTypename)
        {
            return propertyTypename switch
            {
                "Int32" => "int",
                "Int64" => "long",
                "Single" => "float",
                "Boolean" => "bool",
                _ => propertyTypename.ToLowerInvariant(),
            };
        }
    }

    public static string GetFromCardinalityValue(this ErDiagramRelationshipCardinality cardinality)
    {
        return cardinality switch
        {
            ErDiagramRelationshipCardinality.ZeroOrOne => "|o",
            ErDiagramRelationshipCardinality.ExactlyOne => "||",
            ErDiagramRelationshipCardinality.ZeroOrMore => "}o",
            ErDiagramRelationshipCardinality.OneOrMore => "}|",
            _ => throw new ArgumentException(cardinality.ToString(), nameof(cardinality)),
        };
    }

    public static string GetToCardinalityValue(this ErDiagramRelationshipCardinality cardinality)
    {
        return cardinality switch
        {
            ErDiagramRelationshipCardinality.ZeroOrOne => "o|",
            ErDiagramRelationshipCardinality.ExactlyOne => "||",
            ErDiagramRelationshipCardinality.ZeroOrMore => "o{",
            ErDiagramRelationshipCardinality.OneOrMore => "|{",
            _ => throw new ArgumentException(cardinality.ToString(), nameof(cardinality)),
        };
    }

    public static string GetRelationshipLine(this ErDiagramRelationship relationship)
    {
        return relationship.IsIdenifying ? "--" : "..";
    }

    public static bool IsNotMany(this ErDiagramRelationshipCardinality cardinality)
    {
        return cardinality == ErDiagramRelationshipCardinality.ZeroOrOne || cardinality == ErDiagramRelationshipCardinality.ExactlyOne;
    }

    private static readonly Type[] nonPrivitiveAttributeTypes = { typeof(decimal), typeof(string), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid) };
    private static readonly Type[] collectionTypes = { typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>) };
}