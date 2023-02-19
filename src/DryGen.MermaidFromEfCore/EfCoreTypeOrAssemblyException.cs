using System;
using System.Runtime.Serialization;

namespace DryGen.MermaidFromEfCore;

[Serializable]
public sealed class EfCoreTypeOrAssemblyException : Exception
{
    public EfCoreTypeOrAssemblyException(string? message) : base(message)
    {
    }

    private EfCoreTypeOrAssemblyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}