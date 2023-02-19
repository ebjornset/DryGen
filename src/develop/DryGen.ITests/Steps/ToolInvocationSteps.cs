using DryGen.DevUtils.Helpers;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;

namespace DryGen.ITests.Steps;

[Binding]
public sealed class ToolInvocationSteps
{
    private readonly InputFileContext inputFileContext;
    private readonly OptionsFileContext optionsFileContext;
    private int exitCode = 1;
    private string? commandArgs;
    private string processError = string.Empty;
    private string processOutput = string.Empty;

    public ToolInvocationSteps(
        InputFileContext inputFileContext,
        OptionsFileContext optionsFileContext
        )
    {
        this.inputFileContext = inputFileContext;
        this.optionsFileContext = optionsFileContext;
    }

    [When(@"I call the tool with this command line arguments")]
    public void WhenICallTheToolWithThisCommandLineArguments(Table table)
    {
        var argList = table.Rows.Select(x => x["Arg"]).ToList();
        if (inputFileContext.HasInputFileName)
        {
            argList.Add("-i");
            argList.Add(inputFileContext.InputFileName);
        }
        if (optionsFileContext.HasOptionsFile)
        {
            argList.Add("-f");
            argList.Add(optionsFileContext.OptionsFileName);
        }
        var args = string.Join(' ', argList);
        commandArgs = $"{GetDotnetToolCommand()} {args}";
        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = commandArgs,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = GetDotnetWorkingDirectory()
        };
        var process = Process.Start(start);
        if (process == null)
        {
            exitCode = 42;
            return;
        }
        process.WaitForExit();
        using (var reader = process.StandardOutput)
        {
            processOutput = reader.ReadToEnd();
        }
        using (var reader = process.StandardError)
        {
            processError = reader.ReadToEnd();
        }
        exitCode = process.ExitCode;
    }

    [Then(@"I should get exit code '([^']*)' from the tool")]
    public void ThenIShouldGetExitCodeFromTheTool(int expected)
    {
        exitCode.Should().Be(expected, because: $"commandArgs '{commandArgs}'");
    }

    [Then(@"I should get no errors from the tool")]
    public void ThenIShouldGetNoErrorsFromTheTool()
    {
        processError.Should().Be(string.Empty);
    }

    [Then(@"I should get this output from the tool")]
    public void ThenIShouldGetThisOutputFromTheTool(string multilineText)
    {
        processOutput.Should().Be(multilineText);
    }

    private static string GetDotnetToolCommand()
    {
        var runAsTool = Environment.GetEnvironmentVariable("DryGen.ITests.ToolInvocationSteps.RunAsTool");
        if (string.IsNullOrWhiteSpace(runAsTool))
        {
            // Run the application directly, with the version of dotnet that runs the test
            return $"--fx-version {GetDotnetVersion()} DryGen.dll";
        }
        return "dry-gen";
    }

    private static string GetDotnetVersion()
    {
        var netVersion = Environment.Version;
        return $"{netVersion.Major}.{netVersion.Minor}.{netVersion.Build}";
    }

    private static string? GetDotnetWorkingDirectory()
    {
        var workingDirectory = Environment.GetEnvironmentVariable("DryGen.ITests.ToolInvocationSteps.WorkingDirectory");
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return Path.GetDirectoryName(typeof(ToolInvocationSteps).Assembly.Location);
        }
        return Path.GetFullPath(workingDirectory);
    }
}