using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace DryGen
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public sealed class RequiredOptionMissingException : Exception
    {
        public RequiredOptionMissingException(string? message) : base(message)
        {
        }

        private RequiredOptionMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}