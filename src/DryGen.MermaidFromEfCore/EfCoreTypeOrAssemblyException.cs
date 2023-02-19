using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen.MermaidFromEfCore;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class EfCoreTypeOrAssemblyException : Exception
{
    public EfCoreTypeOrAssemblyException(string? message) : base(message)
    {
    }

    private EfCoreTypeOrAssemblyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}