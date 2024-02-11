using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
public sealed class TypeMemberException : Exception
{
    public TypeMemberException(string? message) : base(message)
    {
    }

    [ExcludeFromCodeCoverage]
    private TypeMemberException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}