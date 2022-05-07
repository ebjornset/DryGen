using DryGen.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
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

        public void CompileCodeToMemory(string cSharpCode)
        {
            var assembly = cSharpCode.CompileCodeToMemory(GetReferencedAssemblies());
            this.assembly = assembly;
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
            typeof(KeyAttribute).Assembly, typeof(Expression<>).Assembly, typeof(DbContext).Assembly, typeof(JsonPropertyAttribute).Assembly
        };
    }
}
