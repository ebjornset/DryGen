using System.Collections.Generic;
using DryGen.GithubActions.GhPages;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.NugetPush
{
    public class NugetPushGitHubActionsAttribute : GhPagesGitHubActionsAttribute
    {
        public NugetPushGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var job = base.GetJobs(image, relevantTargets);
            var newSteps = new List<GitHubActionsStep>(job.Steps);
            newSteps.Insert(newSteps.Count - 1, new GitHubActionsNugetPushStep());
            job.Steps = newSteps.ToArray();
            return job;
        }
    }
}
