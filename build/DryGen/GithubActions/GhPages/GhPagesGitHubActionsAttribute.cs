using DryGen.GithubActions.DotNet;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using System.Collections.Generic;

namespace DryGen.GithubActions.GhPages;

public class GhPagesGitHubActionsAttribute : DotNetGitHubActionsAttribute
{
    public GhPagesGitHubActionsAttribute(string name) : base(name)
    {
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        job = GhPagesGitHubActionsJobSetup.ConfigureJob(job, rubyStepOffset: 2, prepareDocsForDeploymentStepOffset: 0);
        return job;
    }
}