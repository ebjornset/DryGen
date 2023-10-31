using CommandLine;
using DryGen.MermaidFromCSharp.ErDiagram;
using YamlDotNet.Serialization;

namespace DryGen.Options;

public abstract class MermaidErDiagramFromCSharpBaseOptions : MermaidFromCSharpBaseOptions, IMermaidErDiagramFromCSharpOptions
{
    [YamlMember(Alias = "exclude-attribute-keytypes", ApplyNamingConventions = false)]
    [Option("exclude-attribute-keytypes", HelpText = "Should attributes key types be excluded from the diagram?")]
    public bool? ExcludeAttributeKeytypes { get; set; }

    [YamlMember(Alias = "exclude-attribute-comments", ApplyNamingConventions = false)]
    [Option("exclude-attribute-comments", HelpText = "Should attributes comments be excluded from the diagram?")]
    public bool? ExcludeAttributeComments { get; set; }

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
            return ErDiagramAttributeTypeExclusion.None;
        }
        set { attributeTypeExclusion = value; }
    }

    [YamlMember(Alias = Constants.MermaidErDiagramFromCsharp.RelationshipTypeExclusionOption, ApplyNamingConventions = false)]
    [Option(Constants.MermaidErDiagramFromCsharp.RelationshipTypeExclusionOption, HelpText = "What kind of relationships should be excluded from the diagram?")]
    public ErDiagramRelationshipTypeExclusion? RelationshipTypeExclusion
    {
        get
        {
            if (relationshipTypeExclusion.HasValue)
            {
                return relationshipTypeExclusion;
            }
            return ErDiagramRelationshipTypeExclusion.None;
        }
        set { relationshipTypeExclusion = value; }
    }

    public ErDiagramAttributeDetailExclusions AttributeDetailExclusions
    {
        get
        {
            var result = ErDiagramAttributeDetailExclusions.None;
            if (ExcludeAttributeKeytypes == true)
            {
                result |= ErDiagramAttributeDetailExclusions.KeyTypes;
            }
            if (ExcludeAttributeComments == true)
            {
                result |= ErDiagramAttributeDetailExclusions.Comments;
            }
            return result;
        }
    }

    public IErDiagramStructureBuilder StructureBuilder { get; private set; }

    protected MermaidErDiagramFromCSharpBaseOptions(IErDiagramStructureBuilder structureBuilder)
    {
        StructureBuilder = structureBuilder;
    }

    private ErDiagramAttributeTypeExclusion? attributeTypeExclusion;
    private ErDiagramRelationshipTypeExclusion? relationshipTypeExclusion;
}
