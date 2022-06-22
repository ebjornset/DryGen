using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions.Configuration;

namespace DryGen.GithubActions.FailOnGitChanges
{
    internal static class FailOnGitChangesGitHubActionsJobSetup
    {
        internal static GitHubActionsJob ConfigureJob(GitHubActionsJob job, int stepsOffset)
        {
            var newSteps = new List<GitHubActionsStep>(job.Steps);
            newSteps.Insert(newSteps.Count - stepsOffset, new GitHubActionsCheckForGitChangesStep());
            newSteps.Insert(newSteps.Count - stepsOffset, new GitHubActionsListGitChangesStep());
            newSteps.Insert(newSteps.Count - stepsOffset, new GitHubActionsDisplayGitDiffsStep());
            newSteps.Insert(newSteps.Count - stepsOffset, new GitHubActionsFailOnGitChangesStep());
            job.Steps = newSteps.ToArray();
            return job;
        }
    }
}
