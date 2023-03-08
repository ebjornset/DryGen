using DryGen.Core;

namespace DryGen.MermaidFromDotnetDepsJson;

internal static class Extensions
{
    internal static string ToInvalidContentMessage(this string message) => $"Invalid deps.json: {message}";
    internal static InvalidContentException ToInvalidContentException(this string message) => new InvalidContentException(message.ToInvalidContentMessage());
}