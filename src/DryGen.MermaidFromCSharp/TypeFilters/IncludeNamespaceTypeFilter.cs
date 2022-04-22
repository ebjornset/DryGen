using DryGen.MermaidFromCSharp;
using System;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class IncludeNamespaceTypeFilter : ITypeFilter
    {
        private readonly string regex;

        public IncludeNamespaceTypeFilter(string regex)
        {
            this.regex = regex;
        }

        public bool Accepts(Type type)
        {
            var match = new Regex(regex).Match(type.Namespace);
            return match.Success;
        }
    }
}
