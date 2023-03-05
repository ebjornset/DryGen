using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp;

public static class Extensions
{
    public static IEnumerable<Type> GetDirectInterfaces(this Type type)
    {
        var allInterfaces = new List<Type>();
        var inheritedInterfaces = new List<Type>();
        foreach (var allInterface in type.GetInterfaces())
        {
            allInterfaces.Add(allInterface);
            foreach (var inheritedInterface in allInterface.GetInterfaces())
                inheritedInterfaces.Add(inheritedInterface);
        }
        var directInterfaces = allInterfaces.Except(inheritedInterfaces);
        if (type.BaseType != null)
        {
            directInterfaces = directInterfaces.Except(type.BaseType.GetInterfaces());
        }
        return directInterfaces;
    }

    public static string GetRelationshipLabel(this string propertyName, string entityName, bool replaceEntityNameAtEndOfPropertyName)
    {
        var endCandidates = new List<string> { entityName, $"{entityName}s", $"{entityName}es" };
        if (entityName.Length > 1)
        {
            endCandidates.Add($"{entityName[..^1]}ies");
        }
        if (endCandidates.Any(X => X == propertyName))
        {
            return string.Empty;
        }
        if (replaceEntityNameAtEndOfPropertyName)
        {
            foreach (var endCandidate in endCandidates.Where(endCandidate => propertyName.EndsWith(endCandidate)))
            {
                propertyName = propertyName[..^endCandidate.Length];
            }
        }
        // Rexex code from https://dotnetfiddle.net/VBuoy7
        var first = Regex
            .Replace(propertyName, "(?<before>[^A-Z])(?<after>([A-Z]))", "${before} ${after}", RegexOptions.Compiled, TimeSpan.FromSeconds(1))
            .Trim();
        var result = Regex
            .Replace(first, "(?<before>[^ ])(?<after>([A-Z][^A-Zs]))", "${before} ${after}", RegexOptions.Compiled, TimeSpan.FromSeconds(1))
            .Trim();
        return result.ToLower();
    }

    public static Regex ToSingleLineCompiledRegexWithTimeout(this string regex)
    {
        return new Regex(regex, RegexOptions.Singleline | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }
}
