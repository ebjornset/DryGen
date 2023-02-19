using System;
using System.Runtime.Serialization;

namespace DryGen.Core;

[Serializable]
public sealed class TypeLoadException : Exception
{
    public TypeLoadException(string? message) : base(message)
    {
    }

    private TypeLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
