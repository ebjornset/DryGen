using DryGen.Core;

namespace DryGen.MermaidFromDotnetDepsJson.Filters;

public class ExcludeAssemblyNameFilter : AbstractStringRegexFilter, IAssemblyNameFilter
{
    public ExcludeAssemblyNameFilter(string regex) : base(regex, shouldMatch: false) { }

    public bool Accepts(string value)
    {
        return DoesAccept(value);
    }
}
