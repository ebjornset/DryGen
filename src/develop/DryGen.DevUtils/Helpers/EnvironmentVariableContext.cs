using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.DevUtils.Helpers;

public sealed class EnvironmentVariableContext : IDisposable
{
    private readonly IList<string> environmentVariables = new List<string>();

    public void SetEnvironmentVariable(string environmentVariable, string? value)
    {
        ValidateEnvironmentVariableForSet(environmentVariable);
        Environment.SetEnvironmentVariable(environmentVariable, value, EnvironmentVariableTarget.Process);
        environmentVariables.Add(environmentVariable);
    }

    public void Dispose()
    {
        RemoveEnvironmentVariables();
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage(Justification = "Just a sanity helper when writing test")]
    private static void ValidateEnvironmentVariableForSet(string environmentVariable)
    {
        if (string.IsNullOrWhiteSpace(environmentVariable))
        {
            throw new ArgumentNullException(nameof(environmentVariable));
        }
    }

    private void RemoveEnvironmentVariables()
    {
        if (environmentVariables.Count > 0)
        {
            foreach (var environmentVariable in environmentVariables)
            {
                Environment.SetEnvironmentVariable(environmentVariable, null, EnvironmentVariableTarget.Process);
            }
            environmentVariables.Clear();
        }
    }
}
