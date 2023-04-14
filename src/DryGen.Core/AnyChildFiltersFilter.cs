using System.Collections.Generic;
using System.Linq;

namespace DryGen.Core;

public class AnyChildFiltersFilter<T> : IFilter<T>
{
    private readonly IReadOnlyList<IFilter<T>> children;

    public AnyChildFiltersFilter(IReadOnlyList<IFilter<T>> children)
    {
        this.children = children;
    }

    public bool Accepts(T value)
    {
        return !children.Any() || children.Any(x => x.Accepts(value));
    }
}
