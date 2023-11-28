using System.Collections.Generic;
using System.Linq;

namespace DryGen.Core;

public class AllChildFiltersFilter<T> : IFilter<T>
{
    private readonly IReadOnlyList<IFilter<T>> children;

    public AllChildFiltersFilter(IReadOnlyList<IFilter<T>> children)
    {
        this.children = children;
    }

    public bool Accepts(T value)
    {
        return !children.Any() || children.All(x => x.Accepts(value));
    }
}