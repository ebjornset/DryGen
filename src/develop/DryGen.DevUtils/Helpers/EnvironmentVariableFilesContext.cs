using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DryGen.DevUtils.Helpers;

public sealed class EnvironmentVariableFilesContext : IDisposable
{
    private readonly IDictionary<string, string> environmentVariableFiles = new Dictionary<string, string>();

    public bool HasEnvironmentVariableFile(string environmentVariable) => environmentVariableFiles.ContainsKey(environmentVariable);

    public string GetEnvironmentVariableFile(string environmentVariable)
    {
        ValidateEnvironmentVariableForGet(environmentVariable);
        return environmentVariableFiles[environmentVariable];
    }

    public void WriteFileAsEnvironmentVariable(string content, string environmentVariable)
    {
        ValidateEnvironmentVariableForSet(environmentVariable);
        var newOptionsFileName = Path.GetTempFileName();
        File.WriteAllText(newOptionsFileName, content);
        environmentVariableFiles[environmentVariable] = newOptionsFileName;
    }

    public void Dispose()
    {
        DeleteEnvironmentVariableFiles();
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage(Justification = "Just a sanity helper when writing test")]
    private void ValidateEnvironmentVariableForGet(string environmentVariable)
    {
        if (!HasEnvironmentVariableFile(environmentVariable))
        {
            throw new ArgumentException($"No file found for '{environmentVariable}'", nameof(environmentVariable));
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Just a sanity helper when writing test")]
    private void ValidateEnvironmentVariableForSet(string environmentVariable)
    {
        if (string.IsNullOrWhiteSpace(environmentVariable))
        {
            throw new ArgumentNullException(nameof(environmentVariable));
        }
        if (environmentVariableFiles.ContainsKey(environmentVariable))
        {
            throw new PropertyAlreadySetException($"File already set for environment variable '{environmentVariable}'");
        }
    }

    private void DeleteEnvironmentVariableFiles()
    {
        if (environmentVariableFiles.Any())
        {
            foreach (var file in environmentVariableFiles.Values)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            environmentVariableFiles.Clear();
        }
    }
}
