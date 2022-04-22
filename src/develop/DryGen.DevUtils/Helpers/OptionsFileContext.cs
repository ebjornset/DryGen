using System;
using System.IO;

namespace DryGen.DevUtils.Helpers
{
    public class OptionsFileContext : IDisposable
    {
        private string? optionsFileName;

        public string OptionsFileName => optionsFileName ?? throw new ArgumentNullException(nameof(optionsFileName));
        public bool HasOptionsFile => optionsFileName != null;

        public void WriteOptionsFile(string yaml)
        {
            TryDeleteOptionsFile();
            var optionsFileName = Path.GetTempFileName();
            File.WriteAllText(optionsFileName, yaml);
            this.optionsFileName = optionsFileName;
        }

        public void Dispose()
        {
            TryDeleteOptionsFile();
            GC.SuppressFinalize(this);
        }

        private void TryDeleteOptionsFile()
        {
            if (!string.IsNullOrEmpty(optionsFileName) && File.Exists(optionsFileName))
            {
                try
                {
                    File.Delete(optionsFileName);
                }
                catch
                {
                    // Best effort to clean up...
                }
                optionsFileName = null;
            }
        }
    }
}
