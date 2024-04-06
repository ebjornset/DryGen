using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using System.Collections.Generic;

namespace DryGen.GithubActions.DotNet;

public class DotNetGitHubActionsAttribute : BaseActionsAttribute
{
    public DotNetGitHubActionsAttribute(string name) : base(name)
    {
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        foreach (var version in new[] { "8.0.*", "7.0.*", "6.0.*" })
        {
            newSteps.Insert(1, new GitHubActionsSetupDotNetStep(version));
        }
        job.Steps = newSteps.ToArray();
        return job;
    }
}