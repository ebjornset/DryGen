using DryGen.DevUtils.Helpers;
using DryGen.MermaidFromCSharp;
using DryGen.UTests.Helpers;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class TypeLoaderByReflectionSteps
    {
        private readonly AssemblyContext assemblyContext;
        private readonly TypeFiltersContext typeFiltersContext;
        private readonly INameRewriter nameRewriter;
        private readonly TypeLoaderByReflection typeLoader;
        private IReadOnlyList<NamedType>? loadedNamedTypes;

        public TypeLoaderByReflectionSteps(TypeLoaderByReflection typeLoader, AssemblyContext assemblyContext, TypeFiltersContext typeFiltersContext, INameRewriter nameRewriter)
        {
            this.typeLoader = typeLoader;
            this.assemblyContext = assemblyContext;
            this.typeFiltersContext = typeFiltersContext;
            this.nameRewriter = nameRewriter;
        }

        [When(@"I load the types to include in the diagram")]
        public void WhenILoadTheTypesToIncludeInTheDiagram()
        {
            loadedNamedTypes = typeLoader.Load(assemblyContext.Assembly, typeFiltersContext.Filters, nameRewriter);
        }

        [Then(@"I get this list of types:")]
        public void ThenIGetThisListOfTypes(Table table)
        {
            table.CompareToSet(loadedNamedTypes?.Select(nt => nt.Type));
        }

        [Then(@"I get '([^']*)' types")]
        public void ThenIGetTypes(int count)
        {
            loadedNamedTypes?.Count.Should().Be(count);
        }
    }
}