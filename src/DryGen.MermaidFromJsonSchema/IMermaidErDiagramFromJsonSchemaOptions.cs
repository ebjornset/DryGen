using DryGen.CSharpFromJsonSchema;

namespace DryGen.MermaidFromJsonSchema;

public interface IMermaidErDiagramFromJsonSchemaOptions : IFromJsonSchemaOptions
{
    bool? ExcludeAllAttributes { get; }
    bool? ExcludeAllRelationships { get; }
}