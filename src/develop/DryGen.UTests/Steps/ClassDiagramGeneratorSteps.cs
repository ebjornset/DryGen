﻿using DryGen.DevUtils.Helpers;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.UTests.Helpers;
using Reqnroll;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class ClassDiagramGeneratorSteps
{
    private readonly AssemblyContext assemblyContext;
    private readonly TypeFiltersContext typeFiltersContext;
    private readonly PropertyFiltersContext propertyFiltersContext;
    private readonly GeneratedRepresentationContext generatedRepresentationContext;
    private readonly TreeShakingContext treeShakingContext;
    private readonly ClassDiagramGenerator generator;

    public ClassDiagramGeneratorSteps(
        ClassDiagramGenerator generator,
        AssemblyContext assemblyContext,
        TypeFiltersContext typeFiltersContext,
        PropertyFiltersContext propertyFiltersContext,
        GeneratedRepresentationContext generatedRepresentationContext,
        TreeShakingContext treeShakingContext)
    {
        this.assemblyContext = assemblyContext;
        this.typeFiltersContext = typeFiltersContext;
        this.propertyFiltersContext = propertyFiltersContext;
        this.generatedRepresentationContext = generatedRepresentationContext;
        this.treeShakingContext = treeShakingContext;
        this.generator = generator;
    }

    [When(@"I generate a Class diagram")]
    public void WhenIGenerateAClassDiagram()
    {
        generatedRepresentationContext.GeneratedRepresentation = generator.Generate(
            assemblyContext.Assembly,
            typeFiltersContext.Filters,
            propertyFiltersContext.Filters,
            nameRewriter: null,
            treeShakingContext.DiagramFilter);
    }
}