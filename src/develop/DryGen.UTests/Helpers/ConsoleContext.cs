using System.IO;

namespace DryGen.UTests.Helpers
{
    public class ConsoleContext
    {
        public TextWriter ErrorWriter { get; private set; }
        public TextWriter OutWriter { get; private set; }

        public ConsoleContext()
        {
            ErrorWriter = new StringWriter();
            OutWriter = new StringWriter();
        }
    }
}
