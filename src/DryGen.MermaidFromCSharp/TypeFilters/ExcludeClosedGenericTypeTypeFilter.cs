using System;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class ExcludeClosedGenericTypeTypeFilter : ITypeFilter
    {
        public bool Accepts(Type type)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                return false;
            }
            return true;
        }
    }
}
