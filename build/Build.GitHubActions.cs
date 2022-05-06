using DryGen.GithubActions;
using Nuke.Common.CI.GitHubActions;

[SonarCloudGitHubActions(
    name: "pr",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] { "main" },
    PublishArtifacts = false,
    InvokedTargets = new[] { nameof(Clean), nameof(UTests), nameof(ITests), nameof(Docs), nameof(Specs) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" }),
]
[SonarCloudGitHubActions(
    name: "build",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    OnWorkflowDispatchOptionalInputs = new[] { "dummy" },
    PublishArtifacts = true,
    InvokedTargets = new[] { nameof(Clean), nameof(UTests), nameof(ITests), nameof(Docs), nameof(Specs) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]

[ReleaseGitHubActions(
    name: "release",
    GitHubActionsImage.UbuntuLatest,
    OnPushTags = new[] { "v*.*.*" },
    PublishArtifacts = true,
    InvokedTargets = new[] { nameof(Clean), nameof(Push) },
    ImportSecrets = new[] { "NUGET_API_KEY" },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]
public partial class Build
{
}
