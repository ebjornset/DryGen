using DryGen.Docs;
using DryGen.UTests.Helpers;
using System.IO;
using Reqnroll;
using DryGen.DevUtils.Helpers;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class ReleaseNotesSteps
{
	private readonly ConsoleContext consoleContext;
	private readonly string releaseNotesTemplatesDirectory;

	public ReleaseNotesSteps(ConsoleContext consoleContext, RootDirectoryContext rootDirectoryContext)
	{
		this.consoleContext = consoleContext;
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

	[When(@"I generate the release notes TOC")]
	public void WhenIGenerateTheReleaseNotesToc()
	{
		ReleaseNotesTocGenerator.Generate(consoleContext.OutWriter, releaseNotesTemplatesDirectory);
	}
}
