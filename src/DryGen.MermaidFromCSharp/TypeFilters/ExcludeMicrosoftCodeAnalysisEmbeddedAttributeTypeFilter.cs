using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class ExcludeMicrosoftCodeAnalysisEmbeddedAttributeTypeFilter : ITypeFilter
{
    public bool Accepts(Type type)
    {
        return type.FullName != "Microsoft.CodeAnalysis.EmbeddedAttribute";
    }
}
