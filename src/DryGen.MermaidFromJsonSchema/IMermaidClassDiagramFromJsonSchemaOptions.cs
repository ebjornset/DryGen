using DryGen.CSharpFromJsonSchema;
using DryGen.MermaidFromCSharp.ClassDiagram;

namespace DryGen.MermaidFromJsonSchema
{
    public interface IMermaidClassDiagramFromJsonSchemaOptions : IFromJsonSchemaOptions
    {
        ClassDiagramDirection? Direction { get; }
    }
}