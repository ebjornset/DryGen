using DryGen.MermaidFromCSharp;
using System;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class IncludeTypeNameTypeFilter : ITypeFilter
    {
        private readonly string regex;

        public IncludeTypeNameTypeFilter(string regex)
        {
            this.regex = regex;
        }

        public bool Accepts(Type type)
        {
            var match = new Regex(regex).Match(type.Name);
            return match.Success;
        }
    }
}
