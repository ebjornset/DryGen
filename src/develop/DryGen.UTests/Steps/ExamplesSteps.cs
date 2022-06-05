using DryGen.Docs;
using DryGen.UTests.Helpers;
using System;
using System.IO;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class ExamplesSteps : IDisposable
    {
        private readonly ConsoleContext consoleContext;
        private readonly string examplesTemplateDirectory;

        public ExamplesSteps(ConsoleContext consoleContext)
        {
            this.consoleContext = consoleContext;
            examplesTemplateDirectory = Path.Combine(Path.GetTempPath(), $"ExamplesTemplateDirectory-{Guid.NewGuid()}");
        }

        [Given(@"the examples template folder contains these files")]
        public void GivenTheExamplesTemplateFolderContainsTheseFiles(Table table)
        {
            Directory.CreateDirectory(examplesTemplateDirectory);
            foreach( var row in table.Rows)
            {
                using var stream = File.Create(Path.Combine(examplesTemplateDirectory, row[0]));
            }
        }

        [When(@"I generate the examples menu")]
        public void WhenIGenerateTheExamplesMenu()
        {
            ExamplesMenuGenerator.Generate(consoleContext.OutWriter, examplesTemplateDirectory);
        }

        public void Dispose()
        {
            DeleteExamplesTemplateDirectory();
            GC.SuppressFinalize(this);
        }

        private void DeleteExamplesTemplateDirectory()
        {
            if (Directory.Exists(examplesTemplateDirectory))
            {
                Directory.Delete(examplesTemplateDirectory, true);
            }
        }
    }
}