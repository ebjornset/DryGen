using System.IO;
using DryGen.Core;
using DryGen.DevUtils.Helpers;
using FluentAssertions;
using Reqnroll;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class DotNetSharedFoldersSteps
{
    private readonly RootDirectoryContext rootDirectoryContext;
    private string? aspNetSharedFolder;
    private string? dotNetSharedFolder;

    public DotNetSharedFoldersSteps(RootDirectoryContext rootDirectoryContext)
    {
        this.rootDirectoryContext = rootDirectoryContext;
    }

    [Given(@"the base .Net shared folder is '([^']*)'")]
    public void GivenTheBaseDotNetShareFolderIs(string dotNetSharedFolder)
    {
        this.dotNetSharedFolder = dotNetSharedFolder;
    }

    [Given(@"these .Net shared folder paths")]
    public void GivenTheseDotNetSharedFolders(Table table)
    {
        table.Header.Count.Should().Be(2);
        foreach(var row in table.Rows) {
            rootDirectoryContext.BuldSubDirectory(rootDirectory => Path.Combine(rootDirectory, Path.Combine(row[0], row[1])));
        }
    }

    [When(@"I resolve the Asp.Net Core shared folder with .Net version '([^']*)'")]
    public void WhenIResolveTheAspDotNetCoreSharedFolderWithDotNetVersion(string dotNetVersion)
    {
        var dotNetRuntimeDirectory = Path.Combine(rootDirectoryContext.RootDirectory, Path.Combine(dotNetSharedFolder.AsNonNull(), dotNetVersion));
        aspNetSharedFolder = new AspNetCoreSharedFolderResolver(dotNetRuntimeDirectory).Resolve();
    }

    [Then(@"I should get the folder Asp.Net Core shared folder '([^']*)'")]
    public void ThenIShouldGetTheFolderAspDotNetCoreSharedFolder(string expected)
    {
        expected = expected.AsLinuxPath();
        aspNetSharedFolder.Should().EndWith(expected);
    }
}
