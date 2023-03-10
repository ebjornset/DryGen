using DryGen.Core;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.MermaidFromDotnetDepsJson;

internal static class Extensions
{
    internal static string ToInvalidDepsJsonMessage(this string message) => $"Invalid deps.json: {message}";

    internal static InvalidContentException ToInvalidContentException(this string message) => new(message.ToInvalidDepsJsonMessage());

    internal static JObject GetPropertyObject(this JProperty property)
    {
        property.CheckForOneChild();
        var objectFirst = property.GetValidFirstChild();
        objectFirst.CheckTypeIsJObject();
        var propertyObject = (JObject)objectFirst;
        return propertyObject;
    }

    internal static JObject GetPropertyObject(this JObject jObject, string propertyName)
    {
        var property = jObject.Property(propertyName);
        property?.CheckForOneChild();
        var propertyFirst = property.GetValidFirstChild();
        propertyFirst.CheckTypeIsJObject();
        var propertyObject = (JObject)propertyFirst;
        return propertyObject;
    }

    internal static JProperty GetFirstProperty(this JObject jObject)
    {
        jObject.CheckForChildren();
        var propertyFirst = jObject.GetValidFirstChild();
        propertyFirst.CheckTypeIs(JTokenType.Property);
        var objectFirstPropert = (JProperty)propertyFirst;
        return objectFirstPropert;
    }

    internal static JToken GetPropertyToken(this JProperty property, JTokenType tokenType)
    {
        property.CheckForOneChild();
        var objectFirst = property.GetValidFirstChild();
        objectFirst.CheckTypeIs(tokenType);
        return objectFirst;
    }

    internal static bool TryGetPropertyObject(this JObject jObject, string propertyName, out JObject? value)
    {
        var property = jObject?.Property(propertyName);
        if (property == null)
        {
            value = null;
            return false;
        }
        property.CheckForOneChild();
        var propertyFirst = property.GetValidFirstChild();
        propertyFirst.CheckTypeIsJObject();
        value = (JObject)propertyFirst;
        return true;
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    internal static JToken GetValidFirstChild(this JToken? token)
    {
        var toeknFirst = token?.First;
        if (toeknFirst == null)
        {
            throw $"Could not get the content in '{token?.Path}'.".ToInvalidContentException();
        }
        return toeknFirst;
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    internal static void CheckTypeIsJObject(this JToken token)
    {
        token?.CheckTypeIs(JTokenType.Object);
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    internal static void CheckTypeIs(this JToken? token, JTokenType tokenType)
    {
        if (token?.Type != tokenType)
        {
            throw $"'{token?.Path}' is of unexpected type '{token?.Type}', expected '{tokenType}'.".ToInvalidContentException();
        }
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    internal static void CheckForOneChild(this JContainer token)
    {
        if (token.Count != 1)
        {
            throw $"'{token.Path}' has unexpected Count '{token.Count}'.".ToInvalidContentException();
        }
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    internal static void CheckForChildren(this JContainer token)
    {
        if (token.Count < 1)
        {
            throw $"'{token.Path}' has unexpected Count '{token.Count}'.".ToInvalidContentException();
        }
    }
}