using System;

namespace DryGen.DevUtils.Helpers;

public sealed class PropertyNotSetException : Exception
{
    public PropertyNotSetException(string message) : base(message) { }
}
