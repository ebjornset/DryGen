using System;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
public sealed class AssemblyLoadException : Exception
{
    public AssemblyLoadException(string? message) : base(message)
    {
    }

    private AssemblyLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}