using System;

namespace DryGen.DevUtils.Helpers
{
    public class PropertyNotSetException : Exception
    {
        public PropertyNotSetException(string message) : base(message) { }
    }
}
