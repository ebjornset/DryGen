using System;

namespace DryGen.Core;

public sealed class TypeMemberException : Exception
{
    public TypeMemberException(string? message) : base(message)
    {
    }
}