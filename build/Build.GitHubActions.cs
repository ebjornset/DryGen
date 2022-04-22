using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

[CustomGitHubActions(
    "pr",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] { "main" },
    PublishArtifacts = false,
    InvokedTargets = new[] { nameof(Clean), nameof(UTests), nameof(ITests), nameof(Docs), nameof(Specs) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" }),
]
[CustomGitHubActions(
    "build",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    OnWorkflowDispatchOptionalInputs = new[] { "dummy" },
    PublishArtifacts = true,
    InvokedTargets = new[] { nameof(Clean), nameof(UTests), nameof(ITests), nameof(Docs), nameof(Specs) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" })
]

[ReleaseGitHubActionsAttribute(
    "release",
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

public class CustomGitHubActionsAttribute : GitHubActionsAttribute
{
    public CustomGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        foreach (var version in new[] { "6.0.*", "3.1.*" })
        {
            newSteps.Insert(1, new GitHubActionsSetupDotNetStep(version));
        }
        job.Steps = newSteps.ToArray();
        return job;
    }
}

public class ReleaseGitHubActionsAttribute : CustomGitHubActionsAttribute
{
    public ReleaseGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsSetupRubyStep("3.1"));
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsGenerateDocsWithJekyllStep());
        newSteps.Add(new GitHubActionsPrepareGeneratedDocsForDeploymentOnBranchGhPagesStep());
        job.Steps = newSteps.ToArray();
        return job;
    }
}

public class GitHubActionsSetupDotNetStep : GitHubActionsStep
{
    public string Version { get; init; }

    public GitHubActionsSetupDotNetStep(string version)
    {
        Version = version;
    }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"- name: Setup .Net {Version}");
        writer.WriteLine("  uses: actions/setup-dotnet@v1");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"dotnet-version: {Version}");
            }
        }
    }
}

public class GitHubActionsSetupRubyStep : GitHubActionsStep
{
    public string Version { get; init; }

    public GitHubActionsSetupRubyStep(string version)
    {
        Version = version;
    }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"- name: Setup Ruby v {Version} (for docs generation with Jekyll)");
        writer.WriteLine("  uses: ruby/setup-ruby@v1");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"ruby-version: {Version}");
            }
        }
    }
}

public class GitHubActionsGenerateDocsWithJekyllStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Generate docs with Jekyll");
        writer.WriteLine("  uses: limjh16/jekyll-action-ts@v2");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("enable_cache: true");
                writer.WriteLine("jekyll_src: ./docs");
            }
        }
    }
}

public class GitHubActionsPrepareGeneratedDocsForDeploymentOnBranchGhPagesStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Prepare generated docs for deployment on branch gh-pages");
        writer.WriteLine("  uses: peaceiris/actions-gh-pages@v3");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("github_token: ${{ secrets.GITHUB_TOKEN }}");
                writer.WriteLine("publish_dir: ./_site");
            }
        }
        writer.WriteLine("# If the Github Pages for the repository is configured correctly");
        writer.WriteLine("# the gh-pages branch should be published automatically within a minute or so...");
    }
}