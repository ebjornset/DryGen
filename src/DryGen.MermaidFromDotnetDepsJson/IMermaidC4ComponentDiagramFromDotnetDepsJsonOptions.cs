using DryGen.Core;

namespace DryGen.MermaidFromDotnetDepsJson;

public interface IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions : IInputFileOptions {
    RelationsLevel RelationsLevel { get; set; }
    BoundariesLevel BoundariesLevel { get; set; }
}