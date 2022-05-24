using DryGen.CSharpFromJsonSchema;
using System;

namespace DryGen.MermaidFromJsonSchema
{
    internal class InternalCSharpFromJsonSchemaOptions : ICSharpFromJsonSchemaOptions
    {
        private readonly IFromJsonSchemaOptions options;

        public InternalCSharpFromJsonSchemaOptions(IFromJsonSchemaOptions options)
        {
            this.options = options;
        }

        public string? Namespace => $"MermaidDiagramFromJsonSchemaGenerator_{Guid.NewGuid().ToString().Replace('-', '_')}";

        public string? ArrayType => null;

        public string? ArrayInstanceType => null;

        public JsonSchemaFileFormat SchemaFileFormat => options.SchemaFileFormat;

        public string? RootClassname => options.RootClassname;

        public string? InputFile => options.InputFile;
    }
}
 