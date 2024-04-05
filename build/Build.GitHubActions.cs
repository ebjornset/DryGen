using DryGen.GithubActions.GhPages;
using DryGen.GithubActions.NugetPush;
using DryGen.GithubActions.SonarCloud;
using Nuke.Common.CI.GitHubActions;

namespace DryGen.Build;

[SonarCloudGitHubActions(
    name: "pr",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPullRequestBranches = new[] { "main" },
    PublishArtifacts = true,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" }),
]
[SonarCloudGitHubActions(
    name: "build",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranches = new[] { "main" },
    OnWorkflowDispatchOptionalInputs = new[] { "dummy" },
    PublishArtifacts = true,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
[NugetPushGitHubActions(
    name: "release",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushTags = new[] { "v*.*.*" },
    PublishArtifacts = true,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
[GhPagesGitHubActions(
    name: "publish-docs",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
public partial class Build
{ }