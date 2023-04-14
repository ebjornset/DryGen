using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class TypeMemberException : Exception
{
    public TypeMemberException(string? message) : base(message) { }

    private TypeMemberException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
