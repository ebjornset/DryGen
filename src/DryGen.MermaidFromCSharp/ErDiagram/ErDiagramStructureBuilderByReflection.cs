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

    private void GeneratieErStructure(IReadOnlyList<ErDiagramEntity> entities, IReadOnlyList<IPropertyFilter> attributeFilters)
    {
        var entityLookup = entities.ToDictionary(x => x.Type, x => x);
        foreach (var entity in entities)
        {
            foreach (var property in entity.Type.GetProperties().Where(p => attributeFilters.All(f => f.Accepts(p))))
            {
                var propertyType = property.PropertyType;
                if (entityLookup.ContainsKey(propertyType))
                {
                    var fromEntity = entityLookup[propertyType];
                    var label = property.Name.Replace(fromEntity.Name, string.Empty).ToLower();
                    fromEntity.AddRelationship(entity, ErDiagramRelationshipCardinality.ExactlyOne, ErDiagramRelationshipCardinality.ExactlyOne, label, property.Name);
                }
                else if (propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition().IsErDiagramRelationshipCollection()
                    && propertyType.GetGenericArguments().Length == 1)
                {
                    var genericArgumentPropertyType = propertyType.GetGenericArguments()[0];
                    if (entityLookup.ContainsKey(genericArgumentPropertyType))
                    {
                        var toEntity = entityLookup[genericArgumentPropertyType];
                        entity.AddRelationship(toEntity, ErDiagramRelationshipCardinality.ExactlyOne, ErDiagramRelationshipCardinality.ZeroOrMore, string.Empty, property.Name);
                    }
                }
                else if (property.IsErDiagramAttributePropertyType())
                {
                    var attributeType = property.GetErDiagramAttributeTypeName();
                    var attributeName = property.Name;
                    var isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null;
                    var isPrimaryKey = property.CustomAttributes.Any(x => x.IsKeyAttribute());
                    entity.AddAttribute(new ErDiagramAttribute(attributeType, attributeName, isNullable, isPrimaryKey));
                }
            }
        }
        foreach (var entity in entities.Reverse())
        {
            entity.RemoveBidirectionalRelationshipDuplicates();
        }
    }
}