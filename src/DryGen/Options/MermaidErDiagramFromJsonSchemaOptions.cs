using CommandLine;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    [Verb(Constants.MermaidErDiagramFromJsonSchema.Verb, HelpText = "Generate a Mermaid Entity Relationship diagram from a Json shcema.")]
    public class MermaidErDiagramFromJsonSchemaOptions : FromJsonSchemaBaseOptions
    {
        [YamlMember(Alias = "exclude-all-attributes", ApplyNamingConventions = false)]
        [Option("exclude-all-attributes", HelpText = "Should all attributes be excluded from the diagram?")]
        public bool? ExcludeAllAttributes { get; set; }

        [YamlMember(Alias = "exclude-all-relationships", ApplyNamingConventions = false)]
        [Option("exclude-all-relationships", HelpText = "Should all relationships be excluded from the diagram?")]
        public bool? ExcludeAllRelationships { get; set; }
    }
}
