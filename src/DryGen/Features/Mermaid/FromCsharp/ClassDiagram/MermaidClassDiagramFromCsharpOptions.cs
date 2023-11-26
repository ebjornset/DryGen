using CommandLine;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.Options;
using YamlDotNet.Serialization;

namespace DryGen.Features.Mermaid.FromCsharp.ClassDiagram;

[Verb(Constants.MermaidClassDiagramFromCsharp.Verb, HelpText = "Generate a Mermaid Class diagram from a C# assembly using reflection.")]
public class MermaidClassDiagramFromCsharpOptions : MermaidFromCsharpBaseOptions, IMermaidClassDiagramFromCSharpOptions
{
    [YamlMember(Alias = "attribute-level", ApplyNamingConventions = false)]
    [Option("attribute-level", HelpText = "What visibility must an attribute have to be included in the diagram?")]
    public ClassDiagramAttributeLevel? AttributeLevel { get; set; }

    [YamlMember(Alias = "method-level", ApplyNamingConventions = false)]
    [Option("method-level", HelpText = "What visibility must a method have to be included in the diagram?")]
    public ClassDiagramMethodLevel? MethodLevel { get; set; }

    [YamlMember(Alias = "direction", ApplyNamingConventions = false)]
    [Option("direction", HelpText = "In what direction should the diagram be generated?")]
    public ClassDiagramDirection? Direction { get; set; }

    [YamlMember(Alias = "exclude-static-attributes", ApplyNamingConventions = false)]
    [Option("exclude-static-attributes", HelpText = "Should static attributes be excluded from the diagram?")]
    public bool? ExcludeStaticAttributes { get; set; }

    [YamlMember(Alias = "exclude-static-methods", ApplyNamingConventions = false)]
    [Option("exclude-static-methods", HelpText = "Should static methods be excluded from the diagram?")]
    public bool? ExcludeStaticMethods { get; set; }

    [YamlMember(Alias = "exclude-method-params", ApplyNamingConventions = false)]
    [Option("exclude-method-params", HelpText = "Should method params be excluded from the diagram? (Replaced by count)")]
    public bool? ExcludeMethodParams { get; set; }
}
