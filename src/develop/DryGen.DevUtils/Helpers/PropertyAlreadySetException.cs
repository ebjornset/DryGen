using System;

namespace DryGen.DevUtils.Helpers;

public sealed class PropertyAlreadySetException : Exception
{
    public PropertyAlreadySetException(string message) : base(message)
    {
    }
}