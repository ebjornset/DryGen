using CommandLine;
using DryGen.MermaidFromCSharp.ErDiagram;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    public abstract class MermaidErDiagramFromCSharpBaseOptions : MermaidFromCSharpBaseOptions, IMermaidErDiagramFromCSharpOptions
    {
        [YamlMember(Alias = Constants.MermaidErDiagramFromCsharp.ExcludeAllAttributesOption, ApplyNamingConventions = false)]
        [Option(Constants.MermaidErDiagramFromCsharp.ExcludeAllAttributesOption, HelpText = Constants.MermaidErDiagramFromCsharp.ReplacedByAttributeTypeExclusionHelpText)]
        public bool? ExcludeAllAttributes { get; set; }

        [YamlMember(Alias = "exclude-attribute-keytypes", ApplyNamingConventions = false)]
        [Option("exclude-attribute-keytypes", HelpText = "Should attributes key types be excluded from the diagram?")]
        public bool? ExcludeAttributeKeytypes { get; set; }

        [YamlMember(Alias = "exclude-attribute-comments", ApplyNamingConventions = false)]
        [Option("exclude-attribute-comments", HelpText = "Should attributes comments be excluded from the diagram?")]
        public bool? ExcludeAttributeComments { get; set; }

        [YamlMember(Alias = Constants.MermaidErDiagramFromCsharp.ExcludeForeignkeyAttributesOption, ApplyNamingConventions = false)]
        [Option(Constants.MermaidErDiagramFromCsharp.ExcludeForeignkeyAttributesOption, HelpText = Constants.MermaidErDiagramFromCsharp.ReplacedByAttributeTypeExclusionHelpText)]
        public bool? ExcludeForeignkeyAttributes { get; set; }

        [YamlMember(Alias = "exclude-all-relationships", ApplyNamingConventions = false)]
        [Option("exclude-all-relationships", HelpText = "Should all relationships be excluded from the diagram?")]
        public bool? ExcludeAllRelationships { get; set; }

        [YamlMember(Alias = Constants.MermaidErDiagramFromCsharp.AttributeTypeExclusionOption, ApplyNamingConventions = false)]
        [Option(Constants.MermaidErDiagramFromCsharp.AttributeTypeExclusionOption, HelpText = "What kind of attributes should be excluded from the diagram?")]
        public ErDiagramAttributeTypeExclusion? AttributeTypeExclusion
        {
            get
            {
                if (attributeTypeExclusion.HasValue)
                {
                    return attributeTypeExclusion;
                }
                if (ExcludeAllAttributes ?? default)
                {
                    return ErDiagramAttributeTypeExclusion.All;
                }
                if (ExcludeForeignkeyAttributes ?? default)
                {
                    return ErDiagramAttributeTypeExclusion.Foreignkeys;
                }
                return ErDiagramAttributeTypeExclusion.None;
            }
            set { attributeTypeExclusion = value; }
        }

        public ErDiagramAttributeDetailExclusions AttributeDetailExclusions
        {
            get
            {
                var result = ErDiagramAttributeDetailExclusions.None;
                if (ExcludeAttributeKeytypes ?? default)
                {
                    result |= ErDiagramAttributeDetailExclusions.KeyTypes;
                }
                if (ExcludeAttributeComments ?? default)
                {
                    result |= ErDiagramAttributeDetailExclusions.Comments;
                }
                return result;
            }
        }

        public ErDiagramRelationshipTypeExclusion? RelationshipTypeExclusion =>
            ExcludeAllRelationships ?? default ? ErDiagramRelationshipTypeExclusion.All : ErDiagramRelationshipTypeExclusion.None;

        public IErDiagramStructureBuilder StructureBuilder { get; private set; }

        protected MermaidErDiagramFromCSharpBaseOptions(IErDiagramStructureBuilder structureBuilder)
        {
            StructureBuilder = structureBuilder;
        }

        private ErDiagramAttributeTypeExclusion? attributeTypeExclusion;
    }
}
