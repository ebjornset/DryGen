﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DryGen.CodeCompiler;

public static class Extensions
{
    public static Assembly CompileCodeToMemory(this string csharpCode, params Assembly[] referencedAssemblies)
    {
        var assemblyName = Path.GetRandomFileName();
        using var ms = new MemoryStream();
        csharpCode.CompileCodeToStream(assemblyName, ms, referencedAssemblies);
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        return assembly;
    }

    public static void CompileCodeToFile(this string csharpCode, string assemblyFileName, params Assembly[] referencedAssemblies)
    {
        var assemblyName = Path.GetFileName(assemblyFileName);
        using var fs = new FileStream(assemblyFileName, FileMode.OpenOrCreate, FileAccess.Write);
        csharpCode.CompileCodeToStream(assemblyName, fs, referencedAssemblies);
    }

    public static void CompileCodeToStream(this string csharpCode, string assemblyName, Stream stream, params Assembly[] referencedAssemblies)
    {
        var dotNetCoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (string.IsNullOrEmpty(dotNetCoreDir))
        {
            throw new ArgumentException($"Could not get directory of 'System.Runtime.dll' from typeof(object)");
        }
        var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
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
        var compilationResult = compilation.Emit(stream);
        compilationResult.ThrowExceptionIfCompilationFailed();
    }

    [ExcludeFromCodeCoverage] //This should in theory never happen in a normal test run, only when we develop new tests that compiles C# code
    private static void ThrowExceptionIfCompilationFailed(this EmitResult compilationResult)
    {
        if (!compilationResult.Success)
        {
            var errors = string.Join(Environment.NewLine, compilationResult.Diagnostics.Select(codeIssue => $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: {codeIssue.Location.GetLineSpan()}, Severity: {codeIssue.Severity} "));
            throw new ArgumentException(errors);
        }
    }
}