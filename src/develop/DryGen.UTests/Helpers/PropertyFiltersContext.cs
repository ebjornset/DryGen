using DryGen.MermaidFromCSharp;
using System.Collections.Generic;

namespace DryGen.UTests.Helpers;

public class PropertyFiltersContext
{
    private readonly List<IPropertyFilter> filters = new();

    public IReadOnlyList<IPropertyFilter> Filters => filters;
}
