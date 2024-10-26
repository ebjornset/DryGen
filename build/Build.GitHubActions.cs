using DryGen.GithubActions.DotNet;
using DryGen.GithubActions.GhPages;
using DryGen.GithubActions.NugetPush;
using Nuke.Common.CI.GitHubActions;

namespace DryGen.Build;

[DotNetGitHubActions(name: "pr", OnPullRequestBranches = ["main"], InvokedTargets = [nameof(CiCd_Build)], PublishArtifacts = false, ImportSecrets = [nameof(SonarToken)])]
[DotNetGitHubActions(name: "build", OnPushBranches = ["main"], OnWorkflowDispatchOptionalInputs = ["dummy"], InvokedTargets = [nameof(CiCd_Build)], PublishArtifacts = false, ImportSecrets = [nameof(SonarToken)])]
[DotNetGitHubActions(name: "tag-version", OnWorkflowDispatchRequiredInputs = ["version"], InvokedTargets = [nameof(CiCd_TagVersion)], PublishArtifacts = false, ImportSecrets = [nameof(SonarToken)], WritePermissions = [GitHubActionsPermissions.Contents, GitHubActionsPermissions.Actions])]
[NugetPushGitHubActions(name: "release", OnPushTags = ["v*.*.*"], InvokedTargets = [nameof(CiCd_Release)], PublishArtifacts = false, ImportSecrets = [nameof(NuGetApiKey), nameof(SonarToken)])]
[GhPagesGitHubActions(name: "publish-docs", On = [GitHubActionsTrigger.WorkflowDispatch], InvokedTargets = [nameof(CiCd_BuildDocs)], PublishArtifacts = false, WritePermissions = [GitHubActionsPermissions.IdToken, GitHubActionsPermissions.Pages], EnvironmentName = "github-pages")]
public partial class Build
{
}