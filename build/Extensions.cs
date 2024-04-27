using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using System;

namespace DryGen.Build;

public static class Extensions
{
    public static bool IsOnVersionTag(this GitRepository repository)
    {
        return repository.Branch?.Contains("refs/tags/v", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }

    public static string ToVersionTagName(this string version)
    {
        return $"v{version}";
    }

    public static AbsolutePath CreateOrCleanDirectory(this AbsolutePath path, bool recurse)
    {
        path.CreateOrCleanDirectory();
        if (recurse)
        {
            path.GlobDirectories("**").ForEach(x => x.DeleteDirectory());
            path.CreateDirectory();
        }
        return path;
    }

    public static string ToQuotedString(this string value)
    {
        return $"\"{value}\"";
    }

}