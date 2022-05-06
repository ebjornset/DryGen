using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

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

public class DotNetGitHubActionsAttribute : GitHubActionsAttribute
{
    public DotNetGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

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

public class SonarCloudGitHubActionsAttribute : DotNetGitHubActionsAttribute
{
    public SonarCloudGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsSetupJavaStep());
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsCacheSonarCloudPackagesStep());
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsCacheSonarCloudScannerStep());
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsInstallSonarCloudScannerStep());
        job.Steps = newSteps.ToArray();
        return job;
    }
}

public class ReleaseGitHubActionsAttribute : SonarCloudGitHubActionsAttribute
{
    public ReleaseGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images) { }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        newSteps.Insert(newSteps.Count - 2, new GitHubActionsSetupRubyStep());
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

public class GitHubActionsSetupJavaStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Setup JDK v 11 (for SonarCloud)");
        writer.WriteLine("  uses: actions/setup-java@v1");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("java-version: 1.11");
            }
        }
    }
}

public class GitHubActionsCacheSonarCloudPackagesStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Cache SonarCloud packages");
        writer.WriteLine("  uses: actions/cache@v2");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine(@"path: ~/sonar/cache");
                writer.WriteLine("key: ${{ runner.os }}-sonar");
                writer.WriteLine("restore-keys: ${{ runner.os }}-sonar");
            }
        }
    }
}

public class GitHubActionsCacheSonarCloudScannerStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Cache SonarCloud scanner");
        writer.WriteLine("  id: cache-sonar-scanner");
        writer.WriteLine("  uses: actions/cache@v2");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine(@"path: ~/.sonar/scanner");
                writer.WriteLine("key: ${{ runner.os }}-sonar-scanner");
                writer.WriteLine("restore-keys: ${{ runner.os }}-sonar-scanner");
            }
        }
    }
}

public class GitHubActionsInstallSonarCloudScannerStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Install SonarCloud scanner");
        writer.WriteLine("  if: steps.cache-sonar-scanner.outputs.cache-hit != 'true'");
        writer.WriteLine("  shell: powershell");
        using (writer.Indent())
        {
            writer.WriteLine("run: |");
            using (writer.Indent())
            {
                writer.WriteLine(@"New-Item -Path ./.sonar/scanner -ItemType Directory");
                writer.WriteLine(@"dotnet tool update dotnet-sonarscanner --tool-path ./.sonar/scanner");
            }
        }
    }
}

public class GitHubActionsSetupRubyStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Setup Ruby v 3.1 (for docs generation with Jekyll)");
        writer.WriteLine("  uses: ruby/setup-ruby@v1");
        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("ruby-version: 3.1");
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