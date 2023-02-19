using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DryGen;

public static class Program
{
    [ExcludeFromCodeCoverage] // No need to test this simple (but a bit test unfrendly) call
    public static int Main(string[] args)
    {
        return Run(args, Console.Out, Console.Error);
    }

    public static int Run(string[] args, TextWriter outWriter, TextWriter errorWeiter)
    {
        return new Generator(outWriter, errorWeiter).Run(args);
    }
}