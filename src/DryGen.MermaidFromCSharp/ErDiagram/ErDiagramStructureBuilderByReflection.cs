using DryGen.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryGen.MermaidFromCSharp.ErDiagram;

public class ErDiagramStructureBuilderByReflection : TypeLoaderByReflection, IErDiagramStructureBuilder
{
    public IReadOnlyList<ErDiagramEntity> GenerateErStructure(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter? nameRewriter)
    {
        var entities = Load(assembly, typeFilters, nameRewriter).Select(x => new ErDiagramEntity(x)).ToList();
        GeneratieErStructure(entities, attributeFilters);
        return entities;
    }

    private static void GeneratieErStructure(IList<ErDiagramEntity> entities, IReadOnlyList<IPropertyFilter> attributeFilters)
	{
		var enumEntities = new Dictionary<Type, ErDiagramEntity>();
		var entityLookup = entities.ToDictionary(x => x.Type, x => x);
		foreach (var entity in entities)
		{
			GeneratieErStructureForEntity(attributeFilters, enumEntities, entityLookup, entity);
		}
		RemoveBidirectionalRelationshipDuplicates(entities);
		MergeTwoOneToManyIntoOneMayToMany(entities);
		enumEntities.AppendToEntities(entities);
	}

	private static void GeneratieErStructureForEntity(IReadOnlyList<IPropertyFilter> attributeFilters, Dictionary<Type, ErDiagramEntity> enumEntities, Dictionary<Type, ErDiagramEntity> entityLookup, ErDiagramEntity entity)
	{
		foreach (var property in entity.Type.GetProperties().Where(p => attributeFilters.All(f => f.Accepts(p))))
		{
			GeneratieErStructureForEntityProperty(enumEntities, entityLookup, entity, property);
		}
	}

	private static void GeneratieErStructureForEntityProperty(Dictionary<Type, ErDiagramEntity> enumEntities, Dictionary<Type, ErDiagramEntity> entityLookup, ErDiagramEntity entity, PropertyInfo? property)
	{
        property = property.AsNonNull();
		var propertyType = property.PropertyType;
		if (entityLookup.TryGetValue(propertyType, out var fromEntity))
		{
			var label = property.Name.Replace(fromEntity.Name, string.Empty).ToLower();
			fromEntity.AddRelationship(entity, ErDiagramRelationshipCardinality.ExactlyOne, ErDiagramRelationshipCardinality.ExactlyOne, label, property.Name);
		}
		else if (propertyType.IsGenericType
			&& propertyType.GetGenericTypeDefinition().IsErDiagramRelationshipCollection()
			&& propertyType.GetGenericArguments().Length == 1)
		{
			var genericArgumentPropertyType = propertyType.GetGenericArguments()[0];
			if (entityLookup.TryGetValue(genericArgumentPropertyType, out var toEntity))
			{
				var label = property.Name.Contains(toEntity.Name) ? string.Empty : property.Name.ToNormalizedRelationshipLabel();
				entity.AddRelationship(toEntity, ErDiagramRelationshipCardinality.ExactlyOne, ErDiagramRelationshipCardinality.ZeroOrMore, label, property.Name);
			}
		}
		else if (property.IsErDiagramAttributePropertyType())
		{
			var attributeType = property.GetErDiagramAttributeTypeName();
			var attributeName = property.Name;
			var nullableUnderlyingType = Nullable.GetUnderlyingType(property.PropertyType);
			var isNullable = nullableUnderlyingType != null;
			var isPrimaryKey = property.CustomAttributes.Any(x => x.IsKeyAttribute());
			var enumType = nullableUnderlyingType ?? propertyType;
			var isEnum = enumType.AddToEntityAsRelationshipIfEnum(attributeName, isNullable, entity, enumEntities, x => new ErDiagramEntity(x.Name, x));
			entity.AddAttribute(new ErDiagramAttribute(attributeType, attributeName, isNullable, isPrimaryKey, isForeignKey: isEnum));
		}
	}

	private static void RemoveBidirectionalRelationshipDuplicates(IList<ErDiagramEntity> entities)
	{
		foreach (var entity in entities.Reverse())
		{
			entity.RemoveBidirectionalRelationshipDuplicates();
		}
	}

	private static void MergeTwoOneToManyIntoOneMayToMany(IList<ErDiagramEntity> entities)
	{
		foreach (var entity in entities)
		{
			entity.MergeTwoOneToManyIntoOneMayToMany();
		}
	}
}