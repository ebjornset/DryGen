using System;
using System.Diagnostics.CodeAnalysis;

#if (NET6_0 || NET7_0)

using System.Runtime.Serialization;

#endif

namespace DryGen;

#if (NET6_0 || NET7_0)

[Serializable]
#endif
public sealed class OptionsException : Exception
{
    public OptionsException(string message) : base(message)
    {
    }

#if (NET6_0 || NET7_0)
    [ExcludeFromCodeCoverage]
    private OptionsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif
}