using DryGen.Core;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;

namespace DryGen.MermaidFromDotnetDepsJson;

internal static class Extensions
{
    internal static string ToInvalidDepsJsonMessage(this string message) => $"Invalid deps.json: {message}";

    internal static InvalidContentException ToInvalidContentException(this string message) => new(message.ToInvalidDepsJsonMessage());

    internal static JsonObject GetPropertyObject(this JsonObject jsonObject, string propertyName)
    {
        jsonObject.TryGetPropertyObject(propertyName, out var propertyObject);
        return propertyObject.CheckForNull();
    }

    internal static string GetFirstPropertyName(this JsonObject jsonObject)
    {
        jsonObject.CheckForChildren();
        string name = string.Empty;
        var enumerator = jsonObject.GetEnumerator();
        if (enumerator.MoveNext())
        {
            name = enumerator.Current.Key;
        }
        return name;
    }

    internal static bool TryGetPropertyObject(this JsonObject jsonObject, string propertyName, out JsonObject? value)
    {
        if (jsonObject?.TryGetPropertyValue(propertyName, out var jsonNode) != true || jsonNode == null)
        {
            value = null;
            return false;
        }
        value = jsonNode.AsObject();
        return true;
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    internal static void CheckForChildren(this JsonObject jsonObject)
    {
        if (!jsonObject.Any())
        {
            throw $"'{jsonObject.GetPath()}' has unexpected Count '{jsonObject.Count}'.".ToInvalidContentException();
        }
    }

    [ExcludeFromCodeCoverage] // Just a guard that we don't bother to test
    internal static JsonObject CheckForNull(this JsonObject? depsJson)
    {
        if (depsJson == null)
        {
            throw $"Could not parse deps.json.".ToInvalidContentException();
        }
        return depsJson;
    }
}