using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class OptionsException : Exception
{
    public OptionsException(string message) : base(message) { }

    private OptionsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
