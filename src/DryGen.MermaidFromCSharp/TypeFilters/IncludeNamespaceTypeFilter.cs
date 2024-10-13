using DryGen.Core;
using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class IncludeNamespaceTypeFilter : AbstractStringRegexFilter, ITypeFilter
{
    public IncludeNamespaceTypeFilter(string regex) : base(regex, shouldMatch: true)
    {
    }

    public bool Accepts(Type type)
    {
        return DoesAccept(type.Namespace.AsNonNull());
    }
}