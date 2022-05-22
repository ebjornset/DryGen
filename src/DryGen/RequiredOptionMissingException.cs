using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class RequiredOptionMissingException : Exception
    {
        public RequiredOptionMissingException()
        {
        }

        public RequiredOptionMissingException(string? message) : base(message)
        {
        }

        public RequiredOptionMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RequiredOptionMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}