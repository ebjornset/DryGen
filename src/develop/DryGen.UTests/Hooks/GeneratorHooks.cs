﻿using BoDi;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.MermaidFromCSharp.EfCore;
using DryGen.MermaidFromCSharp.ErDiagram;
using TechTalk.SpecFlow;

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
            objectContainer.RegisterInstanceAs<INameRewriter>(new NoopNameRewriter());
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
                                                            ClassDiagramAttributeLevel.All,
                                                            ClassDiagramMethodLevel.All,
                                                            ClassDiagramDirection.Default,
                                                            excludeStaticAttributes: false,
                                                            excludeStaticMethods: false,
                                                            excludeMethodParams: false));
        }

        //TODO Can we get rid of this?
        private class NoopNameRewriter : INameRewriter
        {
            public string Rewrite(string name)
            {
                return name;
            }
        }
    }
}
