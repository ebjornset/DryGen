using System;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
public sealed class TypeMemberException : Exception
{
    public TypeMemberException(string? message) : base(message)
    {
    }

    private TypeMemberException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
