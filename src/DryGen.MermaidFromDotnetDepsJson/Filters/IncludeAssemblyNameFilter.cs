using DryGen.Core;

namespace DryGen.MermaidFromDotnetDepsJson.Filters;

public class IncludeAssemblyNameFilter : AbstractStringRegexFilter, IAssemblyNameFilter
{
    public IncludeAssemblyNameFilter(string regex) : base(regex, shouldMatch: true) { }

    public bool Accepts(string value)
    {
        return DoesAccept(value);
    }
}
