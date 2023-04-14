using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class InvalidContentException : Exception
{
    public InvalidContentException(string? message) : base(message) { }

    private InvalidContentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
