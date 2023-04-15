using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class AssemblyLoadException : Exception
{
    public AssemblyLoadException(string? message) : base(message) { }

    private AssemblyLoadException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}