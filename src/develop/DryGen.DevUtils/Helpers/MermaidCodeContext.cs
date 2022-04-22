using System;
using System.IO;

namespace DryGen.DevUtils.Helpers
{
    public class MermaidCodeContext : IDisposable
    {
        private string? mermaidCode;
        private string? mermaidFileName;

        public string MermaidFileName
        {
            get
            {
                if (mermaidFileName == null)
                {
                    mermaidFileName = Path.GetTempFileName();
                }
                return mermaidFileName;
            }
        }

        public string? MermaidCode
        {
            get
            {
                return mermaidCode ?? throw new ArgumentNullException(nameof(mermaidCode));
            }
            set
            {
                mermaidCode = value;
            }
        }

        public string MermaidCodeFromFile
        {
            get
            {
                if (mermaidFileName == null)
                {
                    throw new ArgumentNullException(nameof(mermaidFileName));
                }
                return File.ReadAllText(mermaidFileName);
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(mermaidFileName) && File.Exists(mermaidFileName))
            {
                try
                {
                    File.Delete(mermaidFileName);
                }
                catch
                {
                    // Best effort to clean up...
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
