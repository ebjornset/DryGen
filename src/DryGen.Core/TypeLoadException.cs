using System;

namespace DryGen.Core;

public sealed class TypeLoadException : Exception
{
    public TypeLoadException(string? message) : base(message)
    {
    }

}