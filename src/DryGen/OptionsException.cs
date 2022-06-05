using System;

namespace DryGen
{
    #pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class OptionsException : Exception
    #pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        public OptionsException(string message) : base(message) { }
    }
}
