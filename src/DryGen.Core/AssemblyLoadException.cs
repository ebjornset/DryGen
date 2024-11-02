using System;
namespace DryGen.Core;

public sealed class AssemblyLoadException : Exception
{
    public AssemblyLoadException(string? message) : base(message)
    {
    }

}