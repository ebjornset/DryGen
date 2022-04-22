using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp
{
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
                endCandidates.Add($"{entityName.Substring(0, entityName.Length - 1)}ies");
            }
            if (endCandidates.Any(X => X == propertyName))
            {
                return string.Empty;
            }
            if (replaceEntityNameAtEndOfPropertyName)
            {
                foreach (var endCandidate in endCandidates)
                {
                    if (propertyName.EndsWith(endCandidate))
                    {
                        propertyName = propertyName.Substring(0, propertyName.Length - endCandidate.Length);
                    }
                }
            }
            // Rexex code from https://dotnetfiddle.net/VBuoy7
            var first = System.Text.RegularExpressions.Regex
                .Replace(propertyName, "(?<before>[^A-Z])(?<after>([A-Z]))", "${before} ${after}", System.Text.RegularExpressions.RegexOptions.Compiled)
                .Trim();
            var result = System.Text.RegularExpressions.Regex
                .Replace(first, "(?<before>[^ ])(?<after>([A-Z][^A-Zs]))", "${before} ${after}", System.Text.RegularExpressions.RegexOptions.Compiled)
                .Trim();
            return result.ToLower();
        }
    }
}
