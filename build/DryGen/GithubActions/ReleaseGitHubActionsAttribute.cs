using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions
{
    public class ReleaseGitHubActionsAttribute : SonarCloudGitHubActionsAttribute
    {
        public ReleaseGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var job = base.GetJobs(image, relevantTargets);
            var newSteps = new List<GitHubActionsStep>(job.Steps);
            newSteps.Insert(newSteps.Count - 4, new GitHubActionsSetupRubyStep());
            newSteps.Add(new GitHubActionsGenerateDocsWithJekyllStep());
            newSteps.Add(new GitHubActionsPrepareGeneratedDocsForDeploymentOnBranchGhPagesStep());
            job.Steps = newSteps.ToArray();
            return job;
        }
    }
}
