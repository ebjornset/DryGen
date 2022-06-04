using System;
using System.IO;

namespace DryGen.DevUtils.Helpers
{
    public class InputFileContext : IDisposable
    {
        private string? inputFileName;

        public string InputFileName
        {
            get
            {
                return inputFileName ?? throw new ArgumentNullException(nameof(inputFileName));
            }
            set
            {
                DeleteExistingInputFile();
                inputFileName = value;
            }
        }

        public bool HasInputFileName => !string.IsNullOrEmpty(inputFileName);

        public void Dispose()
        {
            DeleteExistingInputFile();
            GC.SuppressFinalize(this);
        }

        private void DeleteExistingInputFile()
        {
            if (!string.IsNullOrEmpty(inputFileName) && File.Exists(inputFileName))
            {
                File.Delete(inputFileName);
                inputFileName = null;
            }
        }
    }
}
