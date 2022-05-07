using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DryGen.Core
{
    public static class AssemblyExtensions
    {
        public static Assembly CompileCodeToMemory(this string cSharpCode, params Assembly[] referencedAssemblies)
        {
            var assemblyName = Path.GetRandomFileName();
            using var ms = new MemoryStream();
            CompileCodeToStream(cSharpCode, assemblyName, ms, referencedAssemblies);
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());
            return assembly;
        }

        public static void CompileCodeToStream(this string cSharpCode, string assemblyName, Stream stream, params Assembly[] referencedAssemblies)
        {
            var dotNetCoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (string.IsNullOrEmpty(dotNetCoreDir))
            {
                throw new ArgumentException($"Could not get directory of 'System.Runtime.dll' from typeof(object)");
            }
            var syntaxTree = CSharpSyntaxTree.ParseText(cSharpCode);
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "netstandard.dll")),
            }.ToList();
            if (referencedAssemblies != null)
            {
                foreach (var referencedAssembly in referencedAssemblies)
                {
                    var metadataReference = MetadataReference.CreateFromFile(referencedAssembly.Location);
                    references.Add(metadataReference);
                }
            }
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            EmitResult compilationResult = compilation.Emit(stream);
            if (!compilationResult.Success)
            {
                var errors = string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(codeIssue => $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: { codeIssue.Location.GetLineSpan()}, Severity: { codeIssue.Severity} "));
                throw new ArgumentException(errors);
            }
        }
    }
}
