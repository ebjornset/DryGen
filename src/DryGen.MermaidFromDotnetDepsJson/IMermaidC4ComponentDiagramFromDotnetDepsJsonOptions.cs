using DryGen.Core;

namespace DryGen.MermaidFromDotnetDepsJson;

public interface IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions : IInputFileOptions {
    RelationsLevel? RelationsLevel { get; }
    BoundariesLevel? BoundariesLevel { get; }
    bool? ExcludeVersion { get; }
    bool? ExcludeTechn { get; }
    string? Title { get; }
}