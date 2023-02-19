using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.DotNet;

public class DotNetGitHubActionsAttribute : GitHubActionsAttribute
{
    public DotNetGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) {}

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        foreach (var version in new[] { "7.0.*", "6.0.*" })
        {
            newSteps.Insert(1, new GitHubActionsSetupDotNetStep(version));
        }
        job.Steps = newSteps.ToArray();
        return job;
    }
}
