using System.Reflection;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.PropertyFilters
{
    public class ExcludePropertyNamePropertyFilter : IPropertyFilter
    {
        private readonly string regex;

        public ExcludePropertyNamePropertyFilter(string regex)
        {
            this.regex = regex;
        }

        public bool Accepts(PropertyInfo property)
        {
            var match = new Regex(regex).Match(property.Name);
            return !match.Success;
        }
    }
}
