using System.Collections.Generic;
using System.Reflection;

namespace DryGen.MermaidFromCSharp
{
    public interface IDiagramGenerator
    {
        string Generate(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter nameRewriter);
    }
}
