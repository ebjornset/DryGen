using DryGen.Core;

namespace DryGen.CSharpFromJsonSchema
{
    public interface IFromJsonSchemaOptions : IInputFileOptions
    {
        JsonSchemaFileFormat SchemaFileFormat { get; }
        string? RootClassname { get; }
    }
}