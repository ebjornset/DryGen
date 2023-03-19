using CommandLine;
using DryGen.MermaidFromDotnetDepsJson;
using YamlDotNet.Serialization;

namespace DryGen.Options;

[Verb(
    Constants.MermaidC4ComponentDiagramFromDotnetDepsJson.Verb,
    HelpText = "Generate a Mermaid C4 Component diagram from a .Net deps.json file.")]
public class MermaidC4ComponentDiagramFromDotnetDepsJsonOptions : BaseOptions, IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions
{
    [YamlMember(Alias = "relations-level", ApplyNamingConventions = false)]
    [Option("relations-level", HelpText = "What types of dependencies should be included as relations in the diagram? (Default: All)")]
    public RelationsLevel RelationsLevel { get; set; }
    [YamlMember(Alias = "boundaries-level", ApplyNamingConventions = false)]
    [Option("boundaries-level", HelpText = "What kind of 'container boundaries' should be included in the diagram based on the assembly names? (Default: All)")]
    public BoundariesLevel BoundariesLevel { get; set ; }
}
