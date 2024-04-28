﻿using DryGen.Docs;
using DryGen.UTests.Helpers;
using System.IO;
using Reqnroll;
using DryGen.DevUtils.Helpers;
using FluentAssertions;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class ReleaseNotesSteps
{
	private readonly ConsoleContext consoleContext;
	private readonly RootDirectoryContext rootDirectoryContext;
	private readonly string releaseNotesTemplatesDirectory;

	public ReleaseNotesSteps(ConsoleContext consoleContext, RootDirectoryContext rootDirectoryContext)
	{
		this.consoleContext = consoleContext;
		this.rootDirectoryContext = rootDirectoryContext;
		releaseNotesTemplatesDirectory = rootDirectoryContext.BuldSubDirectory(rootDirectory => rootDirectory.AsTemplatesReleaseNotesDirectory());
	}

	[Given(@"the release notes template folder contains these files")]
	public void GivenTheReleaseNotestTemplateFolderContainsTheseFiles(Table table)
	{
		foreach (var row in table.Rows)
		{
			using var stream = File.Create(Path.Combine(releaseNotesTemplatesDirectory, row[0]));
		}
	}

	[Given(@"the release notes template folder contains the file ""([^""]*)"" with content")]
	public void GivenTheReleaseNotesTemplateFolderContainsTheFileWithContent(string fileName, string fileContent)
	{
		var releaseNotestTemplateFile = Path.Combine(releaseNotesTemplatesDirectory, fileName);
		File.WriteAllText(releaseNotestTemplateFile, fileContent);
	}

	[When(@"I generate the release notes TOC")]
	public void WhenIGenerateTheReleaseNotesToc()
	{
		ReleaseNotesTocGenerator.Generate(consoleContext.OutWriter, releaseNotesTemplatesDirectory);
	}

	[When(@"I generate the release notes file ""([^""]*)""")]
	public void WhenIGenerateTheReleaseNotesFile(string fileName)
	{
		ReleaseNotesFileGenerator.Generate(rootDirectoryContext.RootDirectory, fileName);
	}

	[Then(@"the release notes folder should contain the file ""([^""]*)"" with content")]
	public void ThenTheReleaseNotesFolderShouldContainTheFileWithContent(string fileName, string expectedFileContent)
	{
		var releaseNotesGeneratedDirectory = rootDirectoryContext.BuldSubDirectory(rootDirectory => rootDirectory.AsGeneratedReleaseNotesDirectoryCreated());
		var expectedReleaseNotesFile = Path.Combine(releaseNotesGeneratedDirectory, fileName).AsLinuxPath();
		File.Exists(expectedReleaseNotesFile).Should().BeTrue(expectedReleaseNotesFile);
		var actualFileContent = File.ReadAllText(expectedReleaseNotesFile);
		actualFileContent.Should().Be(expectedFileContent);
	}
}
