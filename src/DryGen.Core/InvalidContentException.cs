using System;

namespace DryGen.Core;

public sealed class InvalidContentException : Exception
{
    public InvalidContentException(string? message) : base(message)
    {
    }
}