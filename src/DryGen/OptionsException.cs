using System;

namespace DryGen;

public sealed class OptionsException : Exception
{
    public OptionsException(string message) : base(message)
    {
    }
}