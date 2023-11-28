using DryGen.Core;
using System;
using System.IO;

namespace DryGen.DevUtils.Helpers;

public sealed class GeneratedRepresentationContext : IDisposable
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
            generatedRepresentationFileName ??= Path.GetTempPath().GetRandomFileName();
            return generatedRepresentationFileName;
        }
    }

    public bool HasGeneratedRepresentationFileName => !string.IsNullOrEmpty(generatedRepresentationFileName);

    public string? GeneratedRepresentation
    {
        get
        {
            exceptionContext.ExpectNoException();
            return generatedRepresentation ?? throw new PropertyNotSetException(nameof(GeneratedRepresentation));
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
                throw new PropertyNotSetException(nameof(GeneratedRepresentationFromFile));
            }
            return File.ReadAllText(generatedRepresentationFileName);
        }
    }

    public void WriteGeneratedRepresentationFile(string fileContent)
    {
        File.WriteAllText(GeneratedRepresentationFileName, fileContent);
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(generatedRepresentationFileName) && File.Exists(generatedRepresentationFileName))
        {
            File.Delete(generatedRepresentationFileName);
            generatedRepresentationFileName = null;
        }
        GC.SuppressFinalize(this);
    }
}