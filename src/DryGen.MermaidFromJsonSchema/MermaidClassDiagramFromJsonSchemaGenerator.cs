using DryGen.Core;
using DryGen.CSharpFromJsonSchema;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using System.Threading.Tasks;

namespace DryGen.MermaidFromJsonSchema
{
    public class MermaidClassDiagramFromJsonSchemaGenerator
    {
        public async Task<string> Generate(IMermaidClassDiagramFromJsonSchemaOptions options)
        {
            var cSharpCodeGenerator = new CSharpFromJsonSchemaGenerator();
            var mermaidClassDiagramGenerator = new ClassDiagramGenerator(new TypeLoaderByReflection(), options.Direction, default, default, default, default, default);
            string cSharpCode = await cSharpCodeGenerator.Generate(new InternalCSharpFromJsonSchemaOptions(options));
            var tempAssembly = cSharpCode.CompileCodeToMemory(ReferencedAssemblies.Get());
            var mermaid = mermaidClassDiagramGenerator.Generate(tempAssembly, new ITypeFilter[0], new IPropertyFilter[0], nameRewriter: null);
            return mermaid;
        }
    }
}