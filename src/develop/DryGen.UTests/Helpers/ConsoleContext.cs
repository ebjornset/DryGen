using System.IO;

namespace DryGen.UTests.Helpers;

public class ConsoleContext
{
    public TextWriter ErrorWriter { get; }
    public TextWriter OutWriter { get; }
    public string? ErrorText => ErrorWriter.ToString();
    public string? OutText => OutWriter.ToString();

    public ConsoleContext()
    {
        ErrorWriter = new StringWriter();
        OutWriter = new StringWriter();
    }
}