using DryGen.Core;
using DryGen.CSharpFromJsonSchema;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Threading.Tasks;

namespace DryGen.MermaidFromJsonSchema
{
    public class MermaidClassDiagramFromJsonSchemaGenerator
    {
        public async Task<string> Generate(string? jsonSchemaFileName, JsonSchemaFileFormat jsonSchemaFileFormat, string? rootClassname, ClassDiagramDirection? direction)
        {
            var cSharpCodeGenerator = new CSharpFromJsonSchemaGenerator();
            var mermaidClassDiagramGenerator = new ClassDiagramGenerator(new TypeLoaderByReflection(), direction, default, default, default, default, default);
            var theNamespace = $"MermaidClassDiagramFromJsonSchemaGenerator_{Guid.NewGuid().ToString().Replace('-', '_')}";
            string cSharpCode = await cSharpCodeGenerator.Generate(jsonSchemaFileName, jsonSchemaFileFormat, theNamespace, rootClassname, arrayType: null, arrayInstanceType: null);
            var tempAssembly = cSharpCode.CompileCodeToMemory(typeof(JsonPropertyAttribute).Assembly, typeof(GeneratedCodeAttribute).Assembly);
            var mermaid = mermaidClassDiagramGenerator.Generate(tempAssembly, new ITypeFilter[0], new IPropertyFilter[0], nameRewriter: null);
            return mermaid;
        }
    }
}