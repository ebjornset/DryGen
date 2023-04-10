using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions.Configuration;

namespace DryGen.GithubActions.GhPages;

internal static class GhPagesGitHubActionsJobSetup
{
    internal static GitHubActionsJob ConfigureJob(GitHubActionsJob job, int rubyStepOffset, int jekyllStepOffset, int prepareDocsForDeploymentStepOffset)
    {
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        newSteps.Insert(newSteps.Count - rubyStepOffset, new GitHubActionsSetupRubyStep());
        newSteps.Insert(newSteps.Count - jekyllStepOffset, new GitHubActionsGenerateDocsWithJekyllStep());
        newSteps.Insert(newSteps.Count - prepareDocsForDeploymentStepOffset, new GitHubActionsPrepareGeneratedDocsForDeploymentOnBranchGhPagesStep());
        job.Steps = newSteps.ToArray();
        return job;
    }
}
