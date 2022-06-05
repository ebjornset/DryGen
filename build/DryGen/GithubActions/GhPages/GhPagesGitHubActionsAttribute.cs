﻿using System.Collections.Generic;
using DryGen.GithubActions.SonarCloud;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.GhPages
{
    public class GhPagesGitHubActionsAttribute : SonarCloudGitHubActionsAttribute
    {
        public GhPagesGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var job = base.GetJobs(image, relevantTargets);
            var newSteps = new List<GitHubActionsStep>(job.Steps);
            newSteps.Insert(newSteps.Count - 7, new GitHubActionsSetupRubyStep());
            newSteps.Insert(newSteps.Count - 6, new GitHubActionsGenerateDocsWithJekyllStep());
            newSteps.Add(new GitHubActionsPrepareGeneratedDocsForDeploymentOnBranchGhPagesStep());
            job.Steps = newSteps.ToArray();
            return job;
        }
    }
}
