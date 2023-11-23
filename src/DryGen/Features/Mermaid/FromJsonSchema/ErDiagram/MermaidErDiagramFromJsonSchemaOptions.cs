using CommandLine;
using DryGen.MermaidFromJsonSchema;
using DryGen.Options;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DryGen.Features.Mermaid.FromJsonSchema.ErDiagram;

[Verb(Constants.MermaidErDiagramFromJsonSchema.Verb, HelpText = "Generate a Mermaid Entity Relationship diagram from a Json shcema.")]
public class MermaidErDiagramFromJsonSchemaOptions : FromJsonSchemaBaseOptions, IMermaidErDiagramFromJsonSchemaOptions
{
    [YamlMember(Alias = "exclude-all-attributes", ApplyNamingConventions = false)]
    [Option("exclude-all-attributes", HelpText = "Should all attributes be excluded from the diagram?")]
    public bool? ExcludeAllAttributes { get; set; }

    [YamlMember(Alias = "exclude-all-relationships", ApplyNamingConventions = false)]
    [Option("exclude-all-relationships", HelpText = "Should all relationships be excluded from the diagram?")]
    public bool? ExcludeAllRelationships { get; set; }

    [YamlMember(Alias = "tree-shaking-roots", ApplyNamingConventions = false)]
    [Option("tree-shaking-roots", Separator = ';', HelpText = "A '; separated' list of regular expressions for types to keep as roots when tree shaking the resulting diagram.")]
    public IEnumerable<string>? TreeShakingRoots { get; set; }
}
