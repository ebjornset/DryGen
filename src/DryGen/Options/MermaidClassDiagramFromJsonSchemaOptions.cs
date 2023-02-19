using CommandLine;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.MermaidFromJsonSchema;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DryGen.Options;

[Verb(Constants.MermaidClassDiagramFromJsonSchema.Verb, HelpText = "Generate a Mermaid Class diagram from a json schema.")]
public class MermaidClassDiagramFromJsonSchemaOptions : FromJsonSchemaBaseOptions, IMermaidClassDiagramFromJsonSchemaOptions
{
    [YamlMember(Alias = "direction", ApplyNamingConventions = false)]
    [Option("direction", HelpText = "In what direction should the diagram be generated?")]
    public ClassDiagramDirection? Direction { get; set; }

    [YamlMember(Alias = "tree-shaking-roots", ApplyNamingConventions = false)]
    [Option("tree-shaking-roots", Separator = ';', HelpText = "A list of regular expressions for types to keep as roots when tree shaking the resulting diagram.")]
    public IEnumerable<string>? TreeShakingRoots { get; set; }
}
