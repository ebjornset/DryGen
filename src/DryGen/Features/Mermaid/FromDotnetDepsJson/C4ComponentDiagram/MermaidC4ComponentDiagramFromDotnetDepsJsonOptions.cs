using CommandLine;
using DryGen.MermaidFromDotnetDepsJson;
using DryGen.Options;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DryGen.Features.Mermaid.FromDotnetDepsJson.C4ComponentDiagram;

[Verb(
    Constants.MermaidC4ComponentDiagramFromDotnetDepsJson.Verb,
    HelpText = "Generate a Mermaid C4 Component diagram from a .Net deps.json file.")]
public class MermaidC4ComponentDiagramFromDotnetDepsJsonOptions : CommonInputFileOptions, IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions
{
    [YamlMember(Alias = "relation-level", ApplyNamingConventions = false)]
    [Option("relation-level", HelpText = "What types of dependencies should be included as relations in the diagram? (Default: all)")]
    public RelationLevel? RelationLevel { get; set; }

    [YamlMember(Alias = "boundary-level", ApplyNamingConventions = false)]
    [Option("boundary-level", HelpText = "What kind of 'Container Boundary' should be included in the diagram based on the assembly names? (Default: all)")]
    public BoundaryLevel? BoundaryLevel { get; set; }

    [YamlMember(Alias = "exclude-version", ApplyNamingConventions = false)]
    [Option("exclude-version", HelpText = "Should version information be excluded from the diagram? (Default: false)")]
    public bool? ExcludeVersion { get; set; }

    [YamlMember(Alias = "exclude-techn", ApplyNamingConventions = false)]
    [Option("exclude-techn", HelpText = "Should techn (technonlogy information) be excluded from the diagram? (Default: false)")]
    public bool? ExcludeTechn { get; set; }

    [YamlMember(Alias = "title", ApplyNamingConventions = false)]
    [Option("title", HelpText = "Diagram title. (Default is composed from main assembly and .Net runtime information)")]
    public string? Title { get; set; }

    [YamlMember(Alias = "shape-in-row", ApplyNamingConventions = false)]
    [Option("shape-in-row", HelpText = "Value for the parameter $c4ShapeInRow in UpdateLayoutConfig, used to arrange the diagram layout. (Default: 4)")]
    public int? ShapeInRow { get; set; }

    [YamlMember(Alias = "boundary-in-row", ApplyNamingConventions = false)]
    [Option("boundary-in-row", HelpText = "Value for the parameter $c4BoundaryInRow in UpdateLayoutConfig, used to arrange the diagram layout. (Default: 2)")]
    public int? BoundaryInRow { get; set; }

    [YamlMember(Alias = "include-assemblynames", ApplyNamingConventions = false)]
    [Option("include-assemblynames", Separator = ';', HelpText = "A '; separated' list of regular expressions for name of assemblies to include as components. (Default: all)")]
    public IEnumerable<string>? IncludeAssemblyNames { get; set; }

    [YamlMember(Alias = "exclude-assemblynames", ApplyNamingConventions = false)]
    [Option("exclude-assemblynames", Separator = ';', HelpText = "A '; separated' list of regular expressions for names of assemblies to exclude. (Default: none)")]
    public IEnumerable<string>? ExcludeAssemblyNames { get; set; }
}