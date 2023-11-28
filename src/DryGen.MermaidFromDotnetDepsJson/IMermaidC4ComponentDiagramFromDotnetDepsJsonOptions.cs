using DryGen.Core;

namespace DryGen.MermaidFromDotnetDepsJson;

public interface IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions : IInputFileOptions
{
    RelationLevel? RelationLevel { get; }
    BoundaryLevel? BoundaryLevel { get; }
    bool? ExcludeVersion { get; }
    bool? ExcludeTechn { get; }
    string? Title { get; }
    int? ShapeInRow { get; }
    int? BoundaryInRow { get; }
}