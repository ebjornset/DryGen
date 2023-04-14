using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DryGen.CodeCompiler;

public static class Extensions
{
    public static Assembly CompileCodeToMemory(this string cSharpCode, params Assembly[] referencedAssemblies)
    {
        var assemblyName = Path.GetRandomFileName();
        using var ms = new MemoryStream();
        cSharpCode.CompileCodeToStream(assemblyName, ms, referencedAssemblies);
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
        ThrowExceptionIfCompilationFailed(compilationResult);
    }

    [ExcludeFromCodeCoverage] //This should in theory never happen in a normal test run, only when we develop new tests that compiles C# code
    private static void ThrowExceptionIfCompilationFailed(EmitResult compilationResult)
    {
        if (!compilationResult.Success)
        {
            var errors = string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(codeIssue => $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: {codeIssue.Location.GetLineSpan()}, Severity: {codeIssue.Severity} "));
            throw new ArgumentException(errors);
        }
    }
}
