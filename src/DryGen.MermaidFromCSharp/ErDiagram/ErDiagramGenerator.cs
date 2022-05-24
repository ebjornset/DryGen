using DryGen.MermaidFromCSharp.TypeFilters;
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

        public ErDiagramGenerator(IMermaidErDiagramFromCSharpOptions options) : this(
            options.StructureBuilder,
            options.AttributeTypeExclusion,
            options.AttributeDetailExclusions,
            options.RelationshipTypeExclusion
            )
        { }

        public ErDiagramGenerator(
            IErDiagramStructureBuilder structureBuilder,
            ErDiagramAttributeTypeExclusion attributeTypeExclusion,
            ErDiagramAttributeDetailExclusions attributeDetailExclusions,
            ErDiagramRelationshipTypeExclusion relationshipTypeExclusion
            )
        {
            this.structureBuilder = structureBuilder;
            this.attributeTypeExclusion = attributeTypeExclusion;
            this.attributeDetailExclusions = attributeDetailExclusions;
            this.relationshipTypeExclusion = relationshipTypeExclusion;
        }

        public string Generate(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter? nameRewriter)
        {
            var entities = structureBuilder.GenerateErStructure(assembly, ErDiagramFilters(typeFilters), attributeFilters, nameRewriter);
            var result = GenerateErDiagramMermaid(entities);
            return result;
        }

        private string GenerateErDiagramMermaid(IEnumerable<ErDiagramEntity> entities)
        {
            var sb = new StringBuilder().AppendLine("erDiagram");
            AppendEntitiesToDiagram(entities, sb);
            if (relationshipTypeExclusion != ErDiagramRelationshipTypeExclusion.All)
            {
                AppendRelationshipsToDiagram(entities, sb);
            }
            return sb.ToString();
        }

        private static void AppendRelationshipsToDiagram(IEnumerable<ErDiagramEntity> entities, StringBuilder sb)
        {
            foreach (var entity in entities)
            {
                foreach (var relationship in entity.GetRelationships())
                {
                    sb.Append("\t").Append(entity.Name).Append(' ')
                        .Append(relationship.FromCardinality.GetFromCardinalityValue()).Append(relationship.GetRelationshipLine()).Append(relationship.ToCardinality.GetToCardinalityValue())
                        .Append(' ').Append(relationship.To.Name).AppendLine($" : \"{relationship.Label}\"");
                }
            }
        }

        private void AppendEntitiesToDiagram(IEnumerable<ErDiagramEntity> entities, StringBuilder sb)
        {
            foreach (var entity in entities)
            {
                if (attributeTypeExclusion != ErDiagramAttributeTypeExclusion.All && entity.GetAttributes().Any())
                {
                    AppendAttributeToEnitity(sb, entity);
                }
                else
                {
                    sb.Append("\t").AppendLine(entity.Name);
                }
            }
        }

        private void AppendAttributeToEnitity(StringBuilder sb, ErDiagramEntity entity)
        {
            sb.Append("\t").Append(entity.Name).AppendLine(" {");
            foreach (var attribute in entity.GetAttributes())
            {
                if (attributeTypeExclusion == ErDiagramAttributeTypeExclusion.Foreignkeys && attribute.IsForeignKey)
                {
                    continue;
                }
                sb.Append("\t").Append("\t").Append(attribute.AttributeType).Append(' ').Append(attribute.AttributeName);
                AppendKeyTypeToAttribute(sb, attribute);
                AppendCommentsToAttribute(sb, attribute);
                sb.AppendLine();
            }
            sb.Append("\t").AppendLine("}");
        }

        private void AppendKeyTypeToAttribute(StringBuilder sb, ErDiagramAttribute attribute)
        {
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
        }

        private void AppendCommentsToAttribute(StringBuilder sb, ErDiagramAttribute attribute)
        {
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
        }

        private IReadOnlyList<ITypeFilter> ErDiagramFilters(IReadOnlyList<ITypeFilter> filters)
        {
            var result = new List<ITypeFilter> { new ExcludeAbstractClassTypeFilter(), new ExcludeNonPublicClassTypeFilter(), new ExcludeEnumTypeFilter(), new ExcludeSystemObjectTypeFilter() };
            result.AddRange(filters);
            return result;
        }
    }
}
