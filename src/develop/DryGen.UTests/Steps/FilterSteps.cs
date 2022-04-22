using DryGen.MermaidFromCSharp.TypeFilters;
using DryGen.UTests.Helpers;
using System.Linq;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class FilterSteps
    {
        private readonly TypeFiltersContext typeFiltersContext;

        public FilterSteps(TypeFiltersContext typeFiltersContext)
        {
            this.typeFiltersContext = typeFiltersContext;
        }

        [Given(@"this include namespace filter '([^']*)'")]
        public void GivenThisIncludeNamespaceFilter(string regex)
        {
            var filter = new IncludeNamespaceTypeFilter(regex);
            typeFiltersContext.Add(filter);
        }

        [Given(@"these include namespace filters")]
        public void GivenTheseIncludeNamespaceFilters(Table table)
        {
            var includeNamespaceFilters = table.Rows.Select(x => new IncludeNamespaceTypeFilter(x["Regex"])).ToList();
            var includeAnyNamespaceFilter = new AnyChildFiltersTypeFilter(includeNamespaceFilters);
            typeFiltersContext.Add(includeAnyNamespaceFilter);
        }

        [Given(@"exactly these include namespace filters")]
        public void GivenExactlyTheseIncludeNamespaceFilters(Table table)
        {
            var includeNamespaceFilters = table.Rows.Select(x => new IncludeNamespaceTypeFilter(x["Regex"])).ToList();
            var includeAnyNamespaceFilter = new AnyChildFiltersTypeFilter(includeNamespaceFilters);
            typeFiltersContext.Set(includeAnyNamespaceFilter);
        }

        [Given(@"this exclude type name filter '([^']*)'")]
        public void GivenThisExcludeTypeNameFilter(string regex)
        {
            var filter = new ExcludeTypeNameTypeFilter(regex);
            typeFiltersContext.Add(filter);
        }

        [Given(@"these exclude type name filters")]
        public void GivenTheseExcludeTypeNameFilters(Table table)
        {
            var excludeTypeNameFilters = table.Rows.Select(x => new ExcludeTypeNameTypeFilter(x["Regex"])).ToList();
            var excludeAllTypeNamesFilter = new AllChildFiltersTypeFilter(excludeTypeNameFilters);
            typeFiltersContext.Add(excludeAllTypeNamesFilter);
        }

        [Given(@"this include type name filter '([^']*)'")]
        public void GivenThisIncludeTypeNameFilter(string regex)
        {
            var filter = new IncludeTypeNameTypeFilter(regex);
            typeFiltersContext.Add(filter);
        }

        [Given(@"these include type name filters")]
        public void GivenTheseIncludeTypeNameFilters(Table table)
        {
            var excludeTypeNameFilters = table.Rows.Select(x => new IncludeTypeNameTypeFilter(x["Regex"])).ToList();
            var excludeAllTypeNamesFilter = new AnyChildFiltersTypeFilter(excludeTypeNameFilters);
            typeFiltersContext.Add(excludeAllTypeNamesFilter);
        }
    }
}