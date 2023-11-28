using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class ExcludeAbstractClassTypeFilter : ITypeFilter
{
    public bool Accepts(Type type)
    {
        return !type.IsAbstract;
    }
}