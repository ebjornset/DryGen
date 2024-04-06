using Nuke.Common.CI.GitHubActions;

namespace DryGen.GithubActions;

public abstract class BaseActionsAttribute : GitHubActionsAttribute
{
    protected BaseActionsAttribute(string name) : base(name, GitHubActionsImage.UbuntuLatest)
    {
        FetchDepth = 0;
        CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" };
    }
}
