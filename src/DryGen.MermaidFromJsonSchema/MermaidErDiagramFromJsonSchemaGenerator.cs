using DryGen.Core;
using DryGen.CSharpFromJsonSchema;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ErDiagram;
using System.Threading.Tasks;

namespace DryGen.MermaidFromJsonSchema
{
    public class MermaidErDiagramFromJsonSchemaGenerator
    {
        public async Task<string> Generate(IMermaidErDiagramFromJsonSchemaOptions options)
        {
            var cSharpCodeGenerator = new CSharpFromJsonSchemaGenerator();
            var mermaidErDiagramGenerator = new ErDiagramGenerator(
                new ErDiagramStructureBuilderByReflection(),
                options.ExcludeAllAttributes == true ? ErDiagramAttributeTypeExclusion.All : ErDiagramAttributeTypeExclusion.None,
                ErDiagramAttributeDetailExclusions.None,
                options.ExcludeAllRelationships == true ? ErDiagramRelationshipTypeExclusion.All : ErDiagramRelationshipTypeExclusion.None);
            string cSharpCode = await cSharpCodeGenerator.Generate(new InternalCSharpFromJsonSchemaOptions(options));
            var tempAssembly = cSharpCode.CompileCodeToMemory(ReferencedAssemblies.Get());
            var mermaid = mermaidErDiagramGenerator.Generate(tempAssembly, new ITypeFilter[0], new IPropertyFilter[0], nameRewriter: null);
            return mermaid;
        }
    }
}