namespace DryGen.CSharpFromJsonSchema;

public interface ICSharpFromJsonSchemaOptions : IFromJsonSchemaOptions
{
    string? Namespace { get; }
    string? ArrayType { get; }
    string? ArrayInstanceType { get; }
}