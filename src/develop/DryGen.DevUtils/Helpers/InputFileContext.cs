using System;
using System.IO;

namespace DryGen.DevUtils.Helpers;

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

    public void CreateInputFile(string content, string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentNullException(nameof(content));
        }
        string tmpInputFileName;
        do
        {
            var tmpFileName = Path.GetTempFileName();
            tmpInputFileName = Path.ChangeExtension(tmpFileName, extension);
            if (File.Exists(tmpFileName))
            {
                File.Delete(tmpFileName);
            }
        }
        while (File.Exists(tmpInputFileName));
        if (!string.IsNullOrEmpty(content))
        {
            File.WriteAllText(tmpInputFileName, content);
        }
        InputFileName = tmpInputFileName;
    }

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
