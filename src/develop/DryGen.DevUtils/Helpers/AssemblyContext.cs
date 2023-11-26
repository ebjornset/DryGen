using DryGen.CodeCompiler;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DryGen.DevUtils.Helpers;

public class AssemblyContext
{
    private Assembly? assembly;
    private readonly InputFileContext inputFileContext;
    private readonly EnvironmentVariableFileContext environmentVariableFileContext;

    public AssemblyContext(InputFileContext inputFileContext, EnvironmentVariableFileContext environmentVariableFileContext)
    {
        this.inputFileContext = inputFileContext;
        this.environmentVariableFileContext = environmentVariableFileContext;
    }

    public Assembly Assembly => assembly ?? throw new PropertyNotSetException(nameof(assembly));

    public void CompileCodeToMemory(string cSharpCode)
    {
        assembly = cSharpCode.CompileCodeToMemory(GetReferencedAssemblies());
    }

    public void CompileCodeToFileAsInputFile(string cSharpCode)
    {
        var assemblyFileName = Path.GetRandomFileName();
        cSharpCode.CompileCodeToFile(assemblyFileName, GetReferencedAssemblies());
        inputFileContext.InputFileName = assemblyFileName;
    }

    public void CompileCodeToFileAsEnvironmentVariable(string cSharpCode, string environmentVariable)
    {
        var assemblyFileName = Path.GetRandomFileName();
        cSharpCode.CompileCodeToFile(assemblyFileName, GetReferencedAssemblies());
        environmentVariableFileContext.AddFileAsEnvironmentVariable(assemblyFileName, environmentVariable);
    }

    private static Assembly[] GetReferencedAssemblies() => new[] {
        typeof(Enumerable).Assembly, typeof(Collection<>).Assembly, typeof(KeyAttribute).Assembly, typeof(Expression<>).Assembly, typeof(DbContext).Assembly, typeof(JsonPropertyAttribute).Assembly
    };
}
