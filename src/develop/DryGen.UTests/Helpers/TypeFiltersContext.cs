using DryGen.MermaidFromCSharp;
using System.Collections.Generic;

namespace DryGen.UTests.Helpers;

public class TypeFiltersContext
{
    private readonly List<ITypeFilter> filters = new();

    public IReadOnlyList<ITypeFilter> Filters => filters;

    public void Add(ITypeFilter typeFilter)
    {
        filters.Add(typeFilter);
    }

    public void Set(ITypeFilter typeFilter)
    {
        filters.Clear();
        filters.Add(typeFilter);
    }
}
