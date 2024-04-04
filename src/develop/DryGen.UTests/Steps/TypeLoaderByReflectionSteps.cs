using DryGen.DevUtils.Helpers;
using DryGen.MermaidFromCSharp;
using DryGen.UTests.Helpers;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Reqnroll;
using Reqnroll.Assist;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class TypeLoaderByReflectionSteps
{
    private readonly AssemblyContext assemblyContext;
    private readonly TypeFiltersContext typeFiltersContext;
    private readonly TypeLoaderByReflection typeLoader;
    private IReadOnlyList<NamedType>? loadedNamedTypes;

    public TypeLoaderByReflectionSteps(TypeLoaderByReflection typeLoader, AssemblyContext assemblyContext, TypeFiltersContext typeFiltersContext)
    {
        this.typeLoader = typeLoader;
        this.assemblyContext = assemblyContext;
        this.typeFiltersContext = typeFiltersContext;
    }

    [When(@"I load the types to include in the diagram")]
    public void WhenILoadTheTypesToIncludeInTheDiagram()
    {
        loadedNamedTypes = typeLoader.Load(assemblyContext.Assembly, typeFiltersContext.Filters, nameRewriter: null);
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