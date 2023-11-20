using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.NameRewriters;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class TypeNameSteps
{
    private IReadOnlyList<string>? rewrittenNames;
    private string? rewrittenName;
    private ReplaceNameRewriter? nameRewriter;

    [Given(@"this name replase string '([^']*)' and replacement '([^']*)'")]
    public void GivenThisNameReplaseStringAndReplacement(string replace, string replacement)
    {
        nameRewriter = new ReplaceNameRewriter(replace, replacement);
    }

    [When(@"I rewrite the name '([^']*)'")]
    public void WhenIRewriteTheName(string name)
    {
        rewrittenName = nameRewriter?.Rewrite(name);
    }

    [When(@"I rewrite this list of names:")]
    public void WhenIRewriteThisListOfNames(Table table)
    {
        rewrittenNames = table.CreateSet<string>().Select(x => nameRewriter?.Rewrite(x) ?? x).ToArray();
    }

    [Then(@"I get this list of names:")]
    public void ThenIGetThisListOfNames(Table table)
    {
        var expectedNames = table.CreateSet<string>();
        rewrittenNames.Should().BeEquivalentTo(expectedNames);
    }

    [Then(@"I get the name '([^']*)'")]
    public void ThenIGetTheName(string expectedName)
    {
        rewrittenName.Should().Be(expectedName);
    }
}