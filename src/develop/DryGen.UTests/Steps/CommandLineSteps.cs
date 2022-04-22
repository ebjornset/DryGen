using DryGen.DevUtils.Helpers;
using DryGen.UTests.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class CommandLineSteps
    {
        private readonly AssemblyContext assemblyContext;
        private readonly MermaidCodeContext mermaidCodeContext;
        private readonly OptionsFileContext optionsFileContext;
        private readonly ConsoleContext consoleContext;
        private int? exitCode;
        private IList<string>? args;
        private bool outputIsSpesified;

        public CommandLineSteps(
            AssemblyContext assemblyContext,
            MermaidCodeContext mermaidCodeContext,
            OptionsFileContext optionsFileContext,
            ConsoleContext consoleContext)
        {
            this.assemblyContext = assemblyContext;
            this.mermaidCodeContext = mermaidCodeContext;
            this.optionsFileContext = optionsFileContext;
            this.consoleContext = consoleContext;
        }

        [Given(@"output is spesified as a command line argument")]
        public void GivenOutputIsSpesifiedAsACommandLineArgument()
        {
            outputIsSpesified = true;
        }

        [When(@"I call the program with this command line arguments")]
        public void WhenICallTheProgramWithThisCommandLineArguments(Table table)
        {
            var argList = table.Rows.Select(x => x["Arg"]).ToList();
            if (assemblyContext.HasAssemblyFileName)
            {
                argList.Add("-i");
                argList.Add(assemblyContext.AssemblyFileName);
            }
            if (outputIsSpesified)
            {
                argList.Add("-o");
                argList.Add(mermaidCodeContext.MermaidFileName);
            }
            if (optionsFileContext.HasOptionsFile)
            {
                argList.Add("-f");
                argList.Add(optionsFileContext.OptionsFileName);
            }
            if (args != null)
            {
                argList.AddRange(args);
            }
            args = argList;
            exitCode = Program.Run(argList.ToArray(), consoleContext.OutWriter, consoleContext.ErrorWriter);
        }

        [Then(@"I should get exit code '([^']*)'")]
        public void ThenIShouldGetExitCode(int expected)
        {

            exitCode.Should().Be(expected, because: $"args '{string.Join(' ', args ?? throw new InvalidOperationException(nameof(args)))}'");
        }
    }
}