using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
public sealed class AssemblyLoadException : Exception
{
    public AssemblyLoadException(string? message) : base(message)
    {
    }

    [ExcludeFromCodeCoverage]
    private AssemblyLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}