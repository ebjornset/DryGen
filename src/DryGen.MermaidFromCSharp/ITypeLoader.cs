using System.Collections.Generic;
using System.Reflection;

namespace DryGen.MermaidFromCSharp
{
    public interface ITypeLoader
    {
        IReadOnlyList<NamedType> Load(Assembly assembly, IReadOnlyList<ITypeFilter>? typeFilters, INameRewriter? nameRewriter);
    }
}
