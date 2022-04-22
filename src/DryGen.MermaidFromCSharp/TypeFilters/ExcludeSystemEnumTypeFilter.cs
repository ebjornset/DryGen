using DryGen.MermaidFromCSharp;
using System;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class ExcludeSystemEnumTypeFilter : ITypeFilter
    {
        public bool Accepts(Type type)
        {
            return type != typeof(Enum);
        }
    }
}
