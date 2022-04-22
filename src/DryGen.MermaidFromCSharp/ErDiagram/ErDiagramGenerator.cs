using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DryGen.MermaidFromCSharp.ErDiagram
{
    public class ErDiagramGenerator : IErDiagramGenerator
    {
        private readonly IErDiagramStructureBuilder structureBuilder;
        private readonly ErDiagramAttributeTypeExclusion attributeTypeExclusion;
        private readonly ErDiagramAttributeDetailExclusions attributeDetailExclusions;
        private readonly ErDiagramRelationshipTypeExclusion relationshipTypeExclusion;

        public ErDiagramGenerator(
            IErDiagramStructureBuilder structureBuilder,
            ErDiagramAttributeTypeExclusion attributeTypeExclusion,
            ErDiagramAttributeDetailExclusions attributeDetailExclusions,
            ErDiagramRelationshipTypeExclusion relationshipTypeExclusion)
        {
            this.structureBuilder = structureBuilder;
            this.attributeTypeExclusion = attributeTypeExclusion;
            this.attributeDetailExclusions = attributeDetailExclusions;
            this.relationshipTypeExclusion = relationshipTypeExclusion;
        }

        public string Generate(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter nameRewriter)
        {
            var entities = structureBuilder.GenerateErStructure(assembly, ErDiagramFilters(typeFilters), attributeFilters, nameRewriter);
            var result = GenerateErDiagramMermaid(entities);
            return result;
        }

        private string GenerateErDiagramMermaid(IEnumerable<ErDiagramEntity> entities)
        {
            var sb = new StringBuilder().AppendLine("erDiagram");
            foreach (var entity in entities)
            {
                // Append entities with any attributes
                if (attributeTypeExclusion != ErDiagramAttributeTypeExclusion.All && entity.Attributes.Any())
                {
                    sb.Append("\t").Append(entity.Name).AppendLine(" {");
                    foreach (var attribute in entity.Attributes)
                    {
                        if (attributeTypeExclusion == ErDiagramAttributeTypeExclusion.Foreignkeys && attribute.IsForeignKey)
                        {
                            continue;
                        }
                        sb.Append("\t").Append("\t").Append(attribute.AttributeType).Append(' ').Append(attribute.AttributeName);
                        if (!attributeDetailExclusions.HasFlag(ErDiagramAttributeDetailExclusions.KeyTypes))
                        {
                            if (attribute.IsPrimaryKey)
                            {
                                sb.Append(" PK");
                            }
                            else if (attribute.IsForeignKey)
                            {
                                sb.Append(" FK");
                            }
                        }
                        if (!attributeDetailExclusions.HasFlag(ErDiagramAttributeDetailExclusions.Comments))
                        {
                            if (attribute.IsAlternateKey)
                            {
                                sb.Append(" \"AK\"");
                            }
                            else if (attribute.IsNullable)
                            {
                                sb.Append(" \"Null\"");
                            }
                        }
                        sb.AppendLine();
                    }
                    sb.Append("\t").AppendLine("}");
                }
                else
                {
                    sb.Append("\t").AppendLine(entity.Name);
                }
            }
            if (relationshipTypeExclusion != ErDiagramRelationshipTypeExclusion.All)
            {
                foreach (var entity in entities)
                {
                    // Append relations 
                    foreach (var relationship in entity.Relationships)
                    {
                        sb.Append("\t").Append(entity.Name).Append(' ')
                            .Append(relationship.FromCardinality.GetFromCardinalityValue()).Append(relationship.GetRelationshipLine()).Append(relationship.ToCardinality.GetToCardinalityValue())
                            .Append(' ').Append(relationship.To.Name).AppendLine($" : \"{relationship.Label}\"");
                    }
                }
            }
            return sb.ToString();
        }

        private IReadOnlyList<ITypeFilter> ErDiagramFilters(IReadOnlyList<ITypeFilter> filters)
        {
            var result = new List<ITypeFilter> { new ExcludeAbstractClassTypeFilter(), new ExcludeNonPublicClassTypeFilter(), new ExcludeEnumTypeFilter(), new ExcludeSystemObjectTypeFilter() };
            result.AddRange(filters);
            return result;
        }
    }
}
