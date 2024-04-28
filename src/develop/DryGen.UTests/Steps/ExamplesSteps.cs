using DryGen.Docs;
using DryGen.UTests.Helpers;
using FluentAssertions;
using System.IO;
using Reqnroll;
using DryGen.DevUtils.Helpers;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class ExamplesSteps
{
    private readonly ConsoleContext consoleContext;
	private readonly RootDirectoryContext rootDirectoryContext;
	private readonly string examplesTemplatesDirectory;

    public ExamplesSteps(ConsoleContext consoleContext, RootDirectoryContext rootDirectoryContext)
    {
        this.consoleContext = consoleContext;
		this.rootDirectoryContext = rootDirectoryContext;
        examplesTemplatesDirectory = rootDirectoryContext.BuldSubDirectory( rootDirectory => rootDirectory.AsTemplatesExamplesDirectory());
    }

    [Given(@"the examples template folder contains these files")]
    public void GivenTheExamplesTemplateFolderContainsTheseFiles(Table table)
    {
        foreach (var row in table.Rows)
        {
            using var stream = File.Create(Path.Combine(examplesTemplatesDirectory, row[0]));
        }
    }

    [Given(@"the examples template folder contains the file ""([^""]*)"" with content")]
    public void GivenTheExamplesTemplateFolderContainsTheFileWithContent(string fileName, string fileContent)
    {
        var examplesTemplateFile = Path.Combine(examplesTemplatesDirectory, fileName);
        File.WriteAllText(examplesTemplateFile, fileContent);
    }

    [When(@"I generate the examples TOC")]
    public void WhenIGenerateTheExamplesToc()
    {
        ExamplesTocGenerator.Generate(consoleContext.OutWriter, examplesTemplatesDirectory);
    }

    [When(@"I generate the examples file ""([^""]*)""")]
    public void WhenIGenerateTheExamplesFile(string fileName)
    {
        ExamplesFileGenerator.Generate(rootDirectoryContext.RootDirectory, fileName);
    }

    [Then(@"the examples folder should contain the file ""([^""]*)"" with content")]
    public void ThenTheExamplesFolderShouldContainTheFileWithContent(string fileName, string expectedFileContent)
    {
        var expectedExamplesFile = Path.Combine(rootDirectoryContext.RootDirectory.AsGeneratedExamplesDirectoryCreated(), fileName).AsLinuxPath();
        File.Exists(expectedExamplesFile).Should().BeTrue(expectedExamplesFile);
        var actualFileContent = File.ReadAllText(expectedExamplesFile);
        actualFileContent.Should().Be(expectedFileContent);
    }
}