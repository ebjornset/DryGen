using DryGen.GithubActions.SonarCloud;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using System.Collections.Generic;

namespace DryGen.GithubActions.DotNet;

public class DotNetGitHubActionsAttribute : BaseActionsAttribute
{
    private readonly bool needsJava;

    public DotNetGitHubActionsAttribute(string name, bool needsJava) : base(name)
    {
        this.needsJava = needsJava;
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        if (needsJava)
        {
            newSteps.Insert(1, new SetupJavaStep());
        }
        foreach (var version in new[] { "8.0.*", "7.0.*", "6.0.*" })
        {
            newSteps.Insert(1, new SetupDotNetStep(version));
        }
        job.Steps = newSteps.ToArray();
        return job;
    }
}