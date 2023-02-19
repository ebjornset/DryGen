using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class ExcludeSystemObjectTypeFilter : ITypeFilter
{
    public bool Accepts(Type type)
    {
        return type != typeof(object);
    }
}
