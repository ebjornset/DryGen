using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DryGen.Core;

public static class Extensions
{
    public static Type LoadTypeByName(this string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        throw new TypeLoadException($"Could not load type '{typeName}'");
    }

    public static Regex ToSingleLineCompiledRegexWithTimeout(this string regex)
    {
        return new Regex(regex, RegexOptions.Singleline | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }

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
        ThrowExceptionIfCompilationFailed(compilationResult);
    }

    public static bool IsKeyAttribute(this CustomAttributeData attributeData)
    {
        return attributeData.AttributeType.FullName == "System.ComponentModel.DataAnnotations.KeyAttribute";
    }

    public static bool IsRequiredAttribute(this CustomAttributeData attributeData)
    {
        return attributeData.AttributeType.FullName == "System.ComponentModel.DataAnnotations.RequiredAttribute";
    }

    public static bool IsJsonPropertyRequiredAttribute(this CustomAttributeData attributeData)
    {
        if (attributeData.AttributeType.FullName != "Newtonsoft.Json.JsonPropertyAttribute")
        {
            return false;
        }
        var requiredArgumentValue = attributeData.NamedArguments.Any( x => x.MemberName == "Required") 
            ? attributeData.NamedArguments.Single(x => x.MemberName == "Required").TypedValue.Value 
            : null;
        // 2 is the enum value of "Newtonsoft.Json.Required.Always"
        // NB! This should maybe be made more fool proof?
        return requiredArgumentValue is int requiredArgumentIntValue && requiredArgumentIntValue == 2; 
    }

    public static bool IsRequiredProperty(this PropertyInfo propertyInfo)
    {
        return propertyInfo.CustomAttributes.Any(x => x.IsRequiredAttribute() || x.IsJsonPropertyRequiredAttribute());
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
