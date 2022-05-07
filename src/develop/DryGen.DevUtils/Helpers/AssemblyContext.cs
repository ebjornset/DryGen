using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DryGen.DevUtils.Helpers
{
    public class AssemblyContext
    {
        private Assembly? assembly;
        private readonly InputFileContext inputFileContext;

        public AssemblyContext(InputFileContext inputFileContext)
        {
            this.inputFileContext = inputFileContext;
        }

        public Assembly Assembly => assembly ?? throw new ArgumentNullException(nameof(assembly));

        public void CompileCodeToMemory(string code)
        {
            var assemblyName = Path.GetRandomFileName();
            using var ms = new MemoryStream();
            CompileCodeToStream(code, assemblyName, ms);
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());
            this.assembly = assembly;
        }

        public void CompileCodeToFile(string code)
        {
            var assemblyFileName = Path.GetTempFileName();
            var assemblyName = Path.GetFileName(assemblyFileName);
            using var fs = new FileStream(assemblyFileName, FileMode.OpenOrCreate, FileAccess.Write);
            CompileCodeToStream(code, assemblyName, fs);
            inputFileContext.InputFileName = assemblyFileName;
        }

        private static void CompileCodeToStream(string code, string assemblyName, Stream stream)
        {
            var dotNetCoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(KeyAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Expression<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonPropertyAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir ?? throw new ArgumentException($"Could not get directory of 'System.Runtime.dll' from typeof(object)"), "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir ?? throw new ArgumentException($"Could not get directory of 'netstandard.dll' from typeof(object)"), "netstandard.dll")),
        };
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
