using DryGen.MermaidFromCSharp;
using System;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class ExcludeTypeNameTypeFilter : ITypeFilter
    {
        private readonly string regex;

        public ExcludeTypeNameTypeFilter(string regex)
        {
            this.regex = regex;
        }

        public bool Accepts(Type type)
        {
            var match = new Regex(regex).Match(type.Name);
            return !match.Success;
        }
    }
}
