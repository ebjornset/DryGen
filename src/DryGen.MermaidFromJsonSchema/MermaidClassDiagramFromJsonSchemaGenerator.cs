using DryGen.CodeCompiler;
using DryGen.CSharpFromJsonSchema;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DryGen.MermaidFromJsonSchema;

public class MermaidClassDiagramFromJsonSchemaGenerator
{
    public async Task<string> Generate(IMermaidClassDiagramFromJsonSchemaOptions options, IDiagramFilter diagramFilter)
    {
        var cSharpCodeGenerator = new CSharpFromJsonSchemaGenerator();
        var mermaidClassDiagramGenerator = new ClassDiagramGenerator(new TypeLoaderByReflection(), new MermaidClassDiagramFromCSharpOptions { Direction = options.Direction, Title = options.Title });
        string cSharpCode = await cSharpCodeGenerator.Generate(new InternalCSharpFromJsonSchemaOptions(options));
        var tempAssembly = cSharpCode.CompileCodeToMemory(ReferencedAssemblies.Get());
        var mermaid = mermaidClassDiagramGenerator.Generate(tempAssembly, new ITypeFilter[0], new IPropertyFilter[0], nameRewriter: null, diagramFilter);
        return mermaid;
    }

    private sealed class MermaidClassDiagramFromCSharpOptions : IMermaidClassDiagramFromCSharpOptions
    {
        public ClassDiagramAttributeLevel? AttributeLevel => default;

        public ClassDiagramMethodLevel? MethodLevel => default;

        public ClassDiagramDirection? Direction { get; set; }

        public bool? ExcludeStaticAttributes => default;

        public bool? ExcludeStaticMethods => default;

        public bool? ExcludeMethodParams => default;

        public IEnumerable<string>? IncludeNamespaces => Array.Empty<string>();

        public IEnumerable<string>? IncludeTypeNames => Array.Empty<string>();

        public IEnumerable<string>? ExcludeTypeNames => Array.Empty<string>();

        public IEnumerable<string>? ExcludePropertyNames => Array.Empty<string>();

        public string? NameReplaceFrom => default;

        public string? NameReplaceTo => default;

        public string? Title { get; set; }
    }
}