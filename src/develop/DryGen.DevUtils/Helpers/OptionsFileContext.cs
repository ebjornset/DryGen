﻿using DryGen.Core;
using System;
using System.IO;

namespace DryGen.DevUtils.Helpers;

public sealed class OptionsFileContext : IDisposable
{
    private string? optionsFileName;

    public string OptionsFileName => optionsFileName ?? throw new PropertyNotSetException(nameof(optionsFileName));
    public bool HasOptionsFile => optionsFileName != null;

    public void WriteOptionsFile(string yaml)
    {
        DeleteOptionsFile();
        var newOptionsFileName = Path.GetTempPath().GetRandomFileName();
        File.WriteAllText(newOptionsFileName, yaml);
        optionsFileName = newOptionsFileName;
    }

    public void Dispose()
    {
        DeleteOptionsFile();
        GC.SuppressFinalize(this);
    }

    private void DeleteOptionsFile()
    {
        if (!string.IsNullOrEmpty(optionsFileName) && File.Exists(optionsFileName))
        {
            File.Delete(optionsFileName);
            optionsFileName = null;
        }
    }
}