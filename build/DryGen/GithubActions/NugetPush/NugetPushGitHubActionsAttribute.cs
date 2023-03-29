using System.Collections.Generic;
using DryGen.GithubActions.GhPages;
using DryGen.GithubActions.SonarCloud;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.NugetPush;

public class NugetPushGitHubActionsAttribute : SonarCloudGitHubActionsAttribute
{
    public NugetPushGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        job = GhPagesGitHubActionsJobSetup.ConfigureJob(job, rubyStepOffset: 10, jekyllStepOffset: 7, prepareDocsForDeploymentStepOffset: 1);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsNugetPushStep());
        job.Steps = newSteps.ToArray();
        return job;
    }
}
