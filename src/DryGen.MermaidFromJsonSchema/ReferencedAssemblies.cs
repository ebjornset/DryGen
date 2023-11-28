using Newtonsoft.Json;
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

namespace DryGen.MermaidFromJsonSchema;

internal static class ReferencedAssemblies
{
    internal static Assembly[] Get()
    {
        return new[]
        {
            typeof(JsonPropertyAttribute).Assembly,
            typeof(GeneratedCodeAttribute).Assembly,
            typeof(EnumMemberAttribute).Assembly,
            typeof(RequiredAttribute).Assembly
        };
    }
}