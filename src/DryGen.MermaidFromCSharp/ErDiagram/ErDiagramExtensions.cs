using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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
            && (!property.PropertyType.IsArray || property.PropertyType.IsAssignableFrom(typeof(byte[])))
            && property.PropertyType.IsErDiagramAttributePropertyType();
    }

    public static bool IsErDiagramAttributePropertyType(this Type propertyType)
    {
        if (propertyType.IsPrimitive || propertyType.IsEnum || Array.Exists(nonPrivitiveAttributeTypes, x => x == propertyType))
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
        var typeForName = nullableUnderlyingType ?? type;
        if (typeForName.IsEnum)
        {
            typeForName = typeof(int);
        }
        var propertyTypeName = typeForName.Name;
        var result = GetPropertyTypeName(propertyTypeName);
        return result;

        static string GetPropertyTypeName(string propertyTypeName)
        {
            return propertyTypeName switch
            {
                "Int32" => "int",
                "Int64" => "long",
                "Single" => "float",
                "Boolean" => "bool",
                "Byte[]" => "blob",
                _ => propertyTypeName.ToLowerInvariant(),
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

    public static void AppendToEntities(this Dictionary<Type, ErDiagramEntity> enumEntities, IList<ErDiagramEntity> entities)
    {
        foreach (var enumEntity in enumEntities.Values)
        {
            var attributeTypeName = typeof(int).GetErDiagramAttributeTypeName();
            foreach (var field in enumEntity.Type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumValue = field.GetRawConstantValue();
                enumEntity.AddAttribute(new ErDiagramAttribute(attributeTypeName, field.Name, isNullable: false, isPrimaryKey: false, comments: enumValue?.ToString()));
            }
            entities.Add(enumEntity);
        }
    }

    public static bool AddToEntityAsRelationshipIfEnum(this Type enumType, string attributeName, bool isNullable, ErDiagramEntity entity, Dictionary<Type, ErDiagramEntity> enumEntities)
    {
        var isEnum = enumType.IsEnum;
        if (isEnum)
        {
            if (!enumEntities.TryGetValue(enumType, out var toEntity))
            {
                toEntity = new ErDiagramEntity(enumType.Name, enumType);
                enumEntities[enumType] = toEntity;
            }
            var toCardinality = isNullable ? ErDiagramRelationshipCardinality.ZeroOrOne : ErDiagramRelationshipCardinality.ExactlyOne;
            var label = attributeName != enumType.Name ? attributeName : string.Empty;
            entity.AddRelationship(toEntity, ErDiagramRelationshipCardinality.ZeroOrMore, toCardinality, label, attributeName);
        }
        return isEnum;
    }

    private static readonly Type[] nonPrivitiveAttributeTypes = { typeof(decimal), typeof(string), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid), typeof(byte[]) };
    private static readonly Type[] collectionTypes = { typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>) };
}