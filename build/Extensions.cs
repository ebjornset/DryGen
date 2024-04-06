using Nuke.Common.Git;
using System;

namespace DryGen.Build;

public static class Extensions
{
    public static bool IsOnVersionTag(this GitRepository repository)
    {
        return repository.Branch?.Contains("refs/tags/v", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}