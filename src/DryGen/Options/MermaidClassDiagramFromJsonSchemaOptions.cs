using CommandLine;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.MermaidFromJsonSchema;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    [Verb(Constants.MermaidClassDiagramFromJsonSchema.Verb, HelpText = "Generate a Mermaid Class diagram from a json schema.")]
    public class MermaidClassDiagramFromJsonSchemaOptions : FromJsonSchemaBaseOptions, IMermaidClassDiagramFromJsonSchemaOptions
    {
        [YamlMember(Alias = "direction", ApplyNamingConventions = false)]
        [Option("direction", HelpText = "In which direction should the diagram be generated")]
        public ClassDiagramDirection? Direction { get; set; }
    }
}
