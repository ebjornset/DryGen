using DryGen.GithubActions.DotNet;
using DryGen.GithubActions.GhPages;
using DryGen.GithubActions.NugetPush;
using Nuke.Common.CI.GitHubActions;

namespace DryGen.Build;

[DotNetGitHubActions(
    name: "pr",
    OnPullRequestBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(CiCd_PullRequest) },
    PublishArtifacts = true,
    ImportSecrets = new[] { nameof(SonarToken) }
)]
[DotNetGitHubActions(
    name: "build",
    OnPushBranches = new[] { "main" },
    OnWorkflowDispatchOptionalInputs = new[] { "dummy" },
    InvokedTargets = new[] { nameof(CiCd_Build) },
    PublishArtifacts = true,
    ImportSecrets = new[] { nameof(SonarToken) }
)]
[NugetPushGitHubActions(
    name: "release",
    OnPushTags = new[] { "v*.*.*" },
    InvokedTargets = new[] { nameof(CiCd_Release) },
    PublishArtifacts = true,
    ImportSecrets = new[] { nameof(NuGetApiKey), nameof(SonarToken) }
)]
[GhPagesGitHubActions(
    name: "publish-docs",
    On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(CiCd_BuildDocs) },
    PublishArtifacts = false
)]
public partial class Build
{
}