using System.Collections.Generic;
using DryGen.GithubActions.DotNet;
using DryGen.GithubActions.FailOnGitChanges;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.GhPages
{
    public class GhPagesGitHubActionsAttribute : DotNetGitHubActionsAttribute
    {
        public GhPagesGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var job = base.GetJobs(image, relevantTargets);
            job = GhPagesGitHubActionsJobSetup.ConfigureJob(job, rubyStepOffset: 2,jekyllStepOffset: 0);
            job = FailOnGitChangesGitHubActionsJobSetup.ConfigureJob(job, stepsOffset: 1);
            return job;
        }
    }
}
