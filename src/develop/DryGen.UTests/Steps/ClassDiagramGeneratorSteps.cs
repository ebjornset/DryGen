using DryGen.DevUtils.Helpers;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.UTests.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class ClassDiagramGeneratorSteps
    {
        private readonly AssemblyContext assemblyContext;
        private readonly TypeFiltersContext typeFiltersContext;
        private readonly PropertyFiltersContext propertyFiltersContext;
        private readonly INameRewriter nameRewriter;
        private readonly MermaidCodeContext mermaidCodeContext;
        private readonly ClassDiagramGenerator generator;

        public ClassDiagramGeneratorSteps(
            ClassDiagramGenerator generator,
            AssemblyContext assemblyContext,
            TypeFiltersContext typeFiltersContext,
            PropertyFiltersContext propertyFiltersContext,
            INameRewriter nameRewriter,
            MermaidCodeContext mermaidCodeContext)
        {
            this.assemblyContext = assemblyContext;
            this.typeFiltersContext = typeFiltersContext;
            this.propertyFiltersContext = propertyFiltersContext;
            this.nameRewriter = nameRewriter;
            this.mermaidCodeContext = mermaidCodeContext;
            this.generator = generator;
        }

        [When(@"I generate a Class diagram")]
        public void WhenIGenerateAClassDiagram()
        {
            mermaidCodeContext.MermaidCode = generator.Generate(
                assemblyContext.Assembly,
                typeFiltersContext.Filters,
                propertyFiltersContext.Filters,
                nameRewriter);
        }
    }
}