using DryGen.GithubActions.GhPages;
using DryGen.GithubActions.NugetPush;
using DryGen.GithubActions.SonarCloud;
using Nuke.Common.CI.GitHubActions;

namespace DryGen.Build;

[SonarCloudGitHubActions(
    name: "pr",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] { "main" },
    PublishArtifacts = false,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" }),
]
[SonarCloudGitHubActions(
    name: "build",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    OnWorkflowDispatchOptionalInputs = new[] { "dummy" },
    PublishArtifacts = true,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
[NugetPushGitHubActions(
    name: "release",
    GitHubActionsImage.UbuntuLatest,
    OnPushTags = new[] { "v*.*.*" },
    PublishArtifacts = true,
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
[GhPagesGitHubActions(
    name: "publish-docs",
    GitHubActionsImage.UbuntuLatest,
    OnWorkflowDispatchOptionalInputs = new[] { "dummy" },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
public partial class Build { }
