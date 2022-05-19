using System;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class ExcludeNonPublicClassTypeFilter : ITypeFilter
    {
        public bool Accepts(Type type)
        {
            return type.IsPublic;
        }
    }
}
