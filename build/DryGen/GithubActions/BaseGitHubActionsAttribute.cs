using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using System.Collections.Generic;

namespace DryGen.GithubActions;

public abstract class BaseGitHubActionsAttribute : GitHubActionsAttribute
{
    protected BaseGitHubActionsAttribute(string name) : base(name, GitHubActionsImage.UbuntuLatest)
    {
        FetchDepth = 0;
        CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" };
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        foreach (var version in new[] { "8.0.*" })
        {
            newSteps.Insert(1, new SetupDotNetStep(version));
        }
        job.Steps = newSteps.ToArray();
        return job;
    }
}
