using System.Collections.Generic;
using DryGen.GithubActions.DotNet;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.FailOnGitChanges
{
    public class FailOnGitChangesGitHubActionsAttribute : DotNetGitHubActionsAttribute
    {
        public FailOnGitChangesGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var job = base.GetJobs(image, relevantTargets);
            var newSteps = new List<GitHubActionsStep>(job.Steps);
            newSteps.Add(new GitHubActionsCheckForGitChangesStep());
            newSteps.Add(new GitHubActionsListGitChangesStep());
            newSteps.Add(new GitHubActionsDisplayGitDiffsStep());
            newSteps.Add(new GitHubActionsFailOnGitChangesStep());
            job.Steps = newSteps.ToArray();
            return job;
        }
    }
}
