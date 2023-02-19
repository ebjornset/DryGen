using System.Collections.Generic;
using DryGen.GithubActions.DotNet;
using DryGen.GithubActions.FailOnGitChanges;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace DryGen.GithubActions.SonarCloud;

public class SonarCloudGitHubActionsAttribute : DotNetGitHubActionsAttribute
{
    public SonarCloudGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        job = FailOnGitChangesGitHubActionsJobSetup.ConfigureJob(job, stepsOffset: 0);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        newSteps.Insert(newSteps.Count - 6, new GitHubActionsSetupJavaStep());
        newSteps.Insert(newSteps.Count - 6, new GitHubActionsInstallSonarCloudScannerStep());
        newSteps.Insert(newSteps.Count - 6, new GitHubActionsInstallCoverletReportGeneratorStep());
        newSteps.Insert(newSteps.Count - 6, new GitHubActionsBeginSonarCloudScanStep());
        newSteps.Add(new GitHubActionsRunCoverletReportGeneratorStep());
        newSteps.Add(new GitHubActionsEndSonarCloudScanStep());
        job.Steps = newSteps.ToArray();
        return job;
    }
}
