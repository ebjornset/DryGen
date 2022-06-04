using System;
using System.IO;

namespace DryGen.DevUtils.Helpers
{
    public class GeneratedRepresentationContext : IDisposable
    {
        private readonly ExceptionContext exceptionContext;
        private string? generatedRepresentation;
        private string? generatedRepresentationFileName;

        public GeneratedRepresentationContext(ExceptionContext exceptionContext)
        {
            this.exceptionContext = exceptionContext;
        }

        public string GeneratedRepresentationFileName
        {
            get
            {
                if (generatedRepresentationFileName == null)
                {
                    generatedRepresentationFileName = Path.GetTempFileName();
                }
                return generatedRepresentationFileName;
            }
        }

        public string? GeneratedRepresentation
        {
            get
            {
                exceptionContext.ExpectNoException();
                return generatedRepresentation ?? throw new ArgumentNullException(nameof(generatedRepresentation));
            }
            set
            {
                generatedRepresentation = value;
            }
        }

        public string GeneratedRepresentationFromFile
        {
            get
            {
                if (generatedRepresentationFileName == null)
                {
                    throw new ArgumentNullException(nameof(generatedRepresentationFileName));
                }
                return File.ReadAllText(generatedRepresentationFileName);
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(generatedRepresentationFileName) && File.Exists(generatedRepresentationFileName))
            {
                try
                {
                    File.Delete(generatedRepresentationFileName);
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
