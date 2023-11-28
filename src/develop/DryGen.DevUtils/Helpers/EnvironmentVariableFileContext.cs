using DryGen.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace DryGen.DevUtils.Helpers;

public sealed class EnvironmentVariableFileContext : IDisposable
{
    private readonly IList<string> files = new List<string>();
    private readonly EnvironmentVariableContext environmentVariableContext;

    public EnvironmentVariableFileContext(EnvironmentVariableContext environmentVariableContext)
    {
        this.environmentVariableContext = environmentVariableContext;
    }

    public void WriteFileAsEnvironmentVariable(string content, string environmentVariable)
    {
        environmentVariableContext.ValidateEnvironmentVariableForSet(environmentVariable);
        var newOptionsFileName = Path.GetTempPath().GetRandomFileName();
        File.WriteAllText(newOptionsFileName, content);
        files.Add(newOptionsFileName);
        environmentVariableContext.SetEnvironmentVariable(environmentVariable, newOptionsFileName);
    }

    public void AddFileAsEnvironmentVariable(string fileName, string environmentVariable)
    {
        environmentVariableContext.ValidateEnvironmentVariableForSet(environmentVariable);
        files.Add(fileName);
        environmentVariableContext.SetEnvironmentVariable(environmentVariable, fileName);
    }

    public void Dispose()
    {
        DeleteFiles();
        GC.SuppressFinalize(this);
    }

    private void DeleteFiles()
    {
        if (files.Count > 0)
        {
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            files.Clear();
        }
    }
}