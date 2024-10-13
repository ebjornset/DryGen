using System;

#if (NET6_0)
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
#endif

namespace DryGen;

#if (NET6_0)
[Serializable]
#endif
public sealed class OptionsException : Exception
{
    public OptionsException(string message) : base(message)
    {
    }

#if (NET6_0)
    [ExcludeFromCodeCoverage]
    private OptionsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif
}