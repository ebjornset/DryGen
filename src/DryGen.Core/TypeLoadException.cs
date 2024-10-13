using System;
#if (NET6_0)
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
#endif

namespace DryGen.Core;

#if (NET6_0)
[Serializable]
#endif
public sealed class TypeLoadException : Exception
{
    public TypeLoadException(string? message) : base(message)
    {
    }

#if (NET6_0)
	[ExcludeFromCodeCoverage]
    private TypeLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif
}