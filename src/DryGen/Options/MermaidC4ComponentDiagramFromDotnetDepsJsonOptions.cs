using CommandLine;
using DryGen.MermaidFromDotnetDepsJson;

namespace DryGen.Options;

[Verb(
    Constants.MermaidC4ComponentDiagramFromDotnetDepsJson.Verb,
    HelpText = "Generate a Mermaid C4 Component diagram from a .Net deps.json file.")]
public class MermaidC4ComponentDiagramFromDotnetDepsJsonOptions : BaseOptions, IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions { }
