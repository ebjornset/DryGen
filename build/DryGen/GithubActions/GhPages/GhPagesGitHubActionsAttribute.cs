﻿using DryGen.GithubActions.DotNet;
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
		var newSteps = new List<GitHubActionsStep>(job.Steps)
		{
			new UploadGeneratedDocsAsGhPagesArtifactStep(),
			new DeployGeneratedDocsAsGhPagesArtifactStep(),
		};
		job.Steps = newSteps.ToArray();
		return job;
	}
}