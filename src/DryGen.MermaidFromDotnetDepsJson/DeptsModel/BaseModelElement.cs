using System;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.MermaidFromDotnetDepsJson.DeptsModel;

internal abstract class BaseModelElement
{
    protected BaseModelElement(string id, string delimiter = "/")
    {
        CheckIdNotNull(id);
        Id = id;
        var idParts = id.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
        CheckForTwoIdElementsAfterSplit(id, delimiter, idParts);
        Name = idParts[0];
        Version = idParts[1];
    }

    public string Id { get; }
    public string Name { get; }
    public string Version { get; }

    [ExcludeFromCodeCoverage] // Just a sanity check against future unexcepted format changes that we don't need a test for
    private static void CheckIdNotNull(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
        }
    }

    [ExcludeFromCodeCoverage] // Just a sanity check against future unexcepted format changes that we don't need a test for
    private static void CheckForTwoIdElementsAfterSplit(string id, string delimiter, string[] idParts)
    {
        if (idParts.Length != 2)
        {
            throw $"Could not spilt '{id}' in name and version on delimiter '{delimiter}'".ToInvalidContentException();
        }
    }
}
