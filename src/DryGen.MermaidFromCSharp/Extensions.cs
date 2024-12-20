﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            {
                inheritedInterfaces.Add(inheritedInterface);
            }
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
        if (endCandidates.Contains(propertyName))
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
        return propertyName.ToNormalizedRelationshipLabel();
    }

	public static string ToNormalizedRelationshipLabel(this string relationshipLabel)
	{
		// Rexex code from https://dotnetfiddle.net/VBuoy7
		var first = Regex
			.Replace(relationshipLabel, "(?<before>[^A-Z])(?<after>([A-Z]))", "${before} ${after}", RegexOptions.Compiled, TimeSpan.FromSeconds(1))
			.Trim();
		var result = Regex
			.Replace(first, "(?<before>[^ ])(?<after>([A-Z][^A-Zs]))", "${before} ${after}", RegexOptions.Compiled, TimeSpan.FromSeconds(1))
			.Trim();
		return result.ToLower();
	}

	public static StringBuilder AppendDiagramTitle(this StringBuilder sb, string? title)
    {
        if (!string.IsNullOrEmpty(title))
        {
            sb.AppendLine("---").Append("title: ").AppendLine(title).AppendLine("---");
        }
        return sb;
    }
}