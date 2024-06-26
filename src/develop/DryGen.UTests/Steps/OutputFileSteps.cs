﻿using DryGen.DevUtils.Helpers;
using Reqnroll;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class OutputFileSteps
{
    private readonly GeneratedRepresentationContext generatedRepresentationContext;

    public OutputFileSteps(GeneratedRepresentationContext generatedRepresentationContext)
    {
        this.generatedRepresentationContext = generatedRepresentationContext;
    }

    [Given(@"this output file")]
    public void GivenThisOutputFile(string multilineText)
    {
        generatedRepresentationContext.WriteGeneratedRepresentationFile(multilineText);
    }
}