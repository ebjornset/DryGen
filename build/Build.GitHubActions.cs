using DryGen.GithubActions.DotNet;
using DryGen.GithubActions.GhPages;
using DryGen.GithubActions.NugetPush;
using Nuke.Common.CI.GitHubActions;

namespace DryGen.Build;

[DotNetGitHubActions(name: "pr", OnPullRequestBranches = new[] { "main" }, InvokedTargets = new[] { nameof(CiCd_Build) }, PublishArtifacts = false, ImportSecrets = new[] { nameof(SonarToken) })]
[DotNetGitHubActions(name: "build", OnPushBranches = new[] { "main" }, OnWorkflowDispatchOptionalInputs = new[] { "dummy" }, InvokedTargets = new[] { nameof(CiCd_Build) }, PublishArtifacts = false, ImportSecrets = new[] { nameof(SonarToken) })]
[DotNetGitHubActions(name: "tag-version", OnWorkflowDispatchRequiredInputs = new[] { "version" }, InvokedTargets = new[] { nameof(CiCd_TagVersion) }, PublishArtifacts = false, ImportSecrets = new[] { nameof(SonarToken) }, WritePermissions = new[] { GitHubActionsPermissions.Contents })]
[NugetPushGitHubActions(name: "release", OnPushTags = new[] { "v*.*.*" }, InvokedTargets = new[] { nameof(CiCd_Release) }, PublishArtifacts = false, ImportSecrets = new[] { nameof(NuGetApiKey), nameof(SonarToken) })]
[GhPagesGitHubActions(name: "publish-docs", On = new[] { GitHubActionsTrigger.WorkflowDispatch }, InvokedTargets = new[] { nameof(CiCd_BuildDocs) }, PublishArtifacts = false, WritePermissions = new[] { GitHubActionsPermissions.IdToken })]
public partial class Build
{
}