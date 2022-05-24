using BoDi;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.MermaidFromEfCore;
using DryGen.MermaidFromCSharp.ErDiagram;
using TechTalk.SpecFlow;
using DryGen.Options;

namespace DryGen.UTests.Hooks
{
    [Binding]
    public class GeneratorHooks
    {
        private readonly IObjectContainer objectContainer;
        public ErDiagramAttributeTypeExclusion ErDiagramAttributeTypeExclusion { get; set; }
        public ErDiagramAttributeDetailExclusions ErDiagramAttributeDetailExclusions { get; set; }
        public ErDiagramRelationshipTypeExclusion ErDiagrErDiagramRelationshipTypeExclusion { get; set; }

        public GeneratorHooks(IObjectContainer objectContainer)
        {
            this.objectContainer = objectContainer;
        }

        [BeforeScenario]
        public void InitializeImplementations()
        {
            objectContainer.RegisterTypeAs<TypeLoaderByReflection, ITypeLoader>();
            objectContainer.RegisterFactoryAs(oc => new ErDiagramGenerator(
                                                            new ErDiagramStructureBuilderByReflection(),
                                                            ErDiagramAttributeTypeExclusion,
                                                            ErDiagramAttributeDetailExclusions,
                                                            ErDiagrErDiagramRelationshipTypeExclusion),
                                                    "ErDiagramGeneratorByReflection");
            objectContainer.RegisterFactoryAs(oc => new ErDiagramGenerator(
                                                            new ErDiagramStructureBuilderByEfCore(),
                                                            ErDiagramAttributeTypeExclusion,
                                                            ErDiagramAttributeDetailExclusions,
                                                            ErDiagrErDiagramRelationshipTypeExclusion),
                                                    "ErDiagramGeneratorByEfCore");
            objectContainer.RegisterFactoryAs(oc => new ClassDiagramGenerator(
                                                            oc.Resolve<ITypeLoader>(),
                                                            new MermaidClassDiagramFromCSharpOptions {
                                                                Direction = ClassDiagramDirection.Default,
                                                                AttributeLevel = ClassDiagramAttributeLevel.All,
                                                                MethodLevel=  ClassDiagramMethodLevel.All,
                                                                ExcludeStaticAttributes =  false,
                                                                ExcludeStaticMethods = false,
                                                                ExcludeMethodParams = false 
                                                            }
                                                            ));
        }
    }
}
