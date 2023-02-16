using System;
using System.Runtime.Serialization;

namespace DryGen.MermaidFromEfCore;

[Serializable]
public sealed class EfCoreTypeException : Exception
{
    public EfCoreTypeException(string? message) : base(message)
    {
    }

    private EfCoreTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}