using DryGen.Docs;
using DryGen.UTests.Helpers;
using FluentAssertions;
using System;
using System.IO;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class ExamplesSteps : IDisposable
{
    private readonly ConsoleContext consoleContext;
    private readonly string rootDirectory;
    private readonly string examplesTemplatesDirectory;

    public ExamplesSteps(ConsoleContext consoleContext)
    {
        this.consoleContext = consoleContext;
        rootDirectory = Path.Combine(Path.GetTempPath(), $"dry-gen-test-root-directory-{Guid.NewGuid()}");
        examplesTemplatesDirectory = rootDirectory.AsExamplesTemplatesDirectory();
    }

    [Given(@"the examples template folder contains these files")]
    public void GivenTheExamplesTemplateFolderContainsTheseFiles(Table table)
    {
        Directory.CreateDirectory(examplesTemplatesDirectory);
        foreach (var row in table.Rows)
        {
            using var stream = File.Create(Path.Combine(examplesTemplatesDirectory, row[0]));
        }
    }

    [Given(@"the examples template folder contains the file ""([^""]*)"" with content")]
    public void GivenTheExamplesTemplateFolderContainsTheFileWithContent(string fileName, string fileContent)
    {
        Directory.CreateDirectory(examplesTemplatesDirectory);
        var examplesTemplateFile = Path.Combine(examplesTemplatesDirectory, fileName);
        File.WriteAllText(examplesTemplateFile, fileContent);
    }

    [When(@"I generate the examples menu")]
    public void WhenIGenerateTheExamplesMenu()
    {
        ExamplesMenuGenerator.Generate(consoleContext.OutWriter, examplesTemplatesDirectory);
    }

    [When(@"I generate the examples file ""([^""]*)""")]
    public void WhenIGenerateTheExamplesFile(string fileName)
    {
        Directory.CreateDirectory(rootDirectory.AsExamplesDirectory());
        ExamplesFileGenerator.Generate(rootDirectory, fileName);
    }

    [Then(@"the examples folder should containing the file ""([^""]*)"" with content")]
    public void ThenTheExamplesFolderShouldContainingTheFileWithContent(string fileName, string expectedFileContent)
    {
        var expectedExamplesFile = Path.Combine(rootDirectory.AsExamplesDirectory(), fileName);
        File.Exists(expectedExamplesFile).Should().BeTrue();
        var actualFileContent = File.ReadAllText(expectedExamplesFile);
        actualFileContent.Should().Be(expectedFileContent);
    }

    public void Dispose()
    {
        DeleteExamplesTemplateDirectory();
        GC.SuppressFinalize(this);
    }

    private void DeleteExamplesTemplateDirectory()
    {
        if (Directory.Exists(rootDirectory))
        {
            Directory.Delete(rootDirectory, true);
        }
    }
}