using DryGen.Core;
using DryGen.CSharpFromJsonSchema;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ErDiagram;
using System;
using System.Threading.Tasks;

namespace DryGen.MermaidFromJsonSchema
{
    public class MermaidErDiagramFromJsonSchemaGenerator
    {
        public async Task<string> Generate(string? jsonSchemaFileName, JsonSchemaFileFormat jsonSchemaFileFormat, string? rootClassname, bool? excludeAllAttributes, bool? excludeAllRelationships)
        {
            var cSharpCodeGenerator = new CSharpFromJsonSchemaGenerator();
            var mermaidErDiagramGenerator = new ErDiagramGenerator(
                new ErDiagramStructureBuilderByReflection(),
                excludeAllAttributes == true ? ErDiagramAttributeTypeExclusion.All : ErDiagramAttributeTypeExclusion.None,
                ErDiagramAttributeDetailExclusions.None,
                excludeAllRelationships == true ? ErDiagramRelationshipTypeExclusion.All : ErDiagramRelationshipTypeExclusion.None);
            var theNamespace = $"MermaidErDiagramFromJsonSchemaGenerator_{Guid.NewGuid().ToString().Replace('-', '_')}";
            string cSharpCode = await cSharpCodeGenerator.Generate(jsonSchemaFileName, jsonSchemaFileFormat, theNamespace, rootClassname, arrayType: null, arrayInstanceType: null);
            var tempAssembly = cSharpCode.CompileCodeToMemory(ReferencedAssemblies.Get());
            var mermaid = mermaidErDiagramGenerator.Generate(tempAssembly, new ITypeFilter[0], new IPropertyFilter[0], nameRewriter: null);
            return mermaid;
        }
    }
}