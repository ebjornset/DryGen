using DryGen.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
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

    public AssemblyContext(InputFileContext inputFileContext)
    {
        this.inputFileContext = inputFileContext;
    }

    public Assembly Assembly => assembly ?? throw new ArgumentNullException(nameof(assembly));

    public void CompileCodeToMemory(string cSharpCode)
    {
        assembly = cSharpCode.CompileCodeToMemory(GetReferencedAssemblies());
    }

    public void CompileCodeToFile(string cSharpCode)
    {
        var assemblyFileName = Path.GetTempFileName();
        var assemblyName = Path.GetFileName(assemblyFileName);
        using var fs = new FileStream(assemblyFileName, FileMode.OpenOrCreate, FileAccess.Write);
        cSharpCode.CompileCodeToStream(assemblyName, fs, GetReferencedAssemblies());
        inputFileContext.InputFileName = assemblyFileName;
    }

    private static Assembly[] GetReferencedAssemblies() => new[] { 
        typeof(Enumerable).Assembly, typeof(Collection<>).Assembly, typeof(KeyAttribute).Assembly, typeof(Expression<>).Assembly, typeof(DbContext).Assembly, typeof(JsonPropertyAttribute).Assembly
    };
}
