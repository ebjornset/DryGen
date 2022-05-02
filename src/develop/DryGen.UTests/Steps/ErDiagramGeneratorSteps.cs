using BoDi;
using DryGen.DevUtils.Helpers;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ErDiagram;
using DryGen.UTests.Helpers;
using DryGen.UTests.Hooks;
using System;
using TechTalk.SpecFlow;
using static DryGen.MermaidErDiagramFromCSharpBaseOptions;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class ErDiagramGeneratorSteps
    {
        private readonly IObjectContainer objectContainer;
        private readonly AssemblyContext assemblyContext;
        private readonly TypeFiltersContext typeFiltersContext;
        private readonly PropertyFiltersContext propertyFiltersContext;
        private readonly INameRewriter nameRewriter;
        private readonly GeneratedRepresentationContext generatedRepresentationContext;
        private readonly GeneratorHooks generatorHooks;
        private readonly ExceptionContext exceptionContext;

        public ErDiagramGeneratorSteps(
            IObjectContainer objectContainer,
            AssemblyContext assemblyContext,
            TypeFiltersContext typeFiltersContext,
            PropertyFiltersContext propertyFiltersContext,
            INameRewriter nameRewriter,
            GeneratedRepresentationContext generatedRepresentationContext,
            GeneratorHooks generatorHooks,
            ExceptionContext exceptionContext)
        {
            this.objectContainer = objectContainer;
            this.assemblyContext = assemblyContext;
            this.typeFiltersContext = typeFiltersContext;
            this.propertyFiltersContext = propertyFiltersContext;
            this.nameRewriter = nameRewriter;
            this.generatedRepresentationContext = generatedRepresentationContext;
            this.generatorHooks = generatorHooks;
            this.exceptionContext = exceptionContext;
        }

        [Given(@"the Er diagram attribute type exclusion '([^']*)'")]
        public void GivenTheErDiagramAttributeTypeExclusion(string typeExclusion)
        {
            generatorHooks.ErDiagramAttributeTypeExclusion = (ErDiagramAttributeTypeExclusion)Enum.Parse(typeof(ErDiagramAttributeTypeExclusion), typeExclusion);
        }

        [Given(@"the Er diagram relationship exclusion '([^']*)'")]
        public void GivenTheErDiagramRelationshipExclusion(string level)
        {
            generatorHooks.ErDiagrErDiagramRelationshipTypeExclusion = (ErDiagramRelationshipTypeExclusion)Enum.Parse(typeof(ErDiagramRelationshipTypeExclusion), level);
        }

        [When(@"I generate an ER diagram using reflection")]
        public void WhenIGenerateAnERDiagramUsingReflection()
        {
            var erDiagramGeneratorByReflection = objectContainer.Resolve<ErDiagramGenerator>("ErDiagramGeneratorByReflection");
            generatedRepresentationContext.GeneratedRepresentation = exceptionContext.HarvestExceptionFrom(() =>
                erDiagramGeneratorByReflection.Generate(
                    assemblyContext.Assembly,
                    typeFiltersContext.Filters,
                    propertyFiltersContext.Filters,
                    nameRewriter));
        }

        [When(@"I generate an ER diagram using EF Core")]
        public void WhenIGenerateAnERDiagramUsingEfCore()
        {
            var erDiagramGeneratorByEfCore = objectContainer.Resolve<ErDiagramGenerator>("ErDiagramGeneratorByEfCore"); ;
            generatedRepresentationContext.GeneratedRepresentation = exceptionContext.HarvestExceptionFrom(() =>
                erDiagramGeneratorByEfCore.Generate(
                    assemblyContext.Assembly,
                    typeFiltersContext.Filters,
                    propertyFiltersContext.Filters,
                    nameRewriter));
        }

        [When(@"I generate an ER diagram using '([^']*)'")]
        public void WhenIGenerateAnERDiagramUsing(string structureBuilder)
        {
            var structureBuilderType = (ErStructureBuilderType)Enum.Parse(typeof(ErStructureBuilderType), structureBuilder);
            if (structureBuilderType == ErStructureBuilderType.Reflection)
            {
                WhenIGenerateAnERDiagramUsingReflection();
            }
            else if (structureBuilderType == ErStructureBuilderType.EfCore)
            {
                WhenIGenerateAnERDiagramUsingEfCore();
            }
            else
            {
                throw new ArgumentException($"Unsupported structure builder type '{structureBuilderType}'");
            }
        }
    }
}