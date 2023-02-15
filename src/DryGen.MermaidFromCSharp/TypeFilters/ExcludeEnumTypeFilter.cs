using System;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class ExcludeEnumTypeFilter : ITypeFilter
    {
        public bool Accepts(Type type)
        {
            return !type.IsEnum;
        }
    }
}
