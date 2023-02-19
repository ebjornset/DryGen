using System.IO;

namespace DryGen.Docs;

public static class Extensions
{
    public static string AsCommandLineReplaceToken(this string replaceToken)
    {
        return $".!.!.replace-token-for-{replaceToken.ToLowerInvariant()}-commandline.!.!.";
    }

    public static string AsGeneratedRepresentationReplaceToken(this string replaceToken)
    {
        return $".!.!.replace-token-for-{replaceToken.ToLowerInvariant()}.!.!.";
    }

    public static string AsDocsDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory, "docs").AsLinuxPath();
    }

    public static string AsTemplatesDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsDocsDirectory(), "_templates").AsLinuxPath();
    }

    public static string AsExamplesTemplatesDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsTemplatesDirectory(), "examples").AsLinuxPath();
    }

    public static string AsExamplesDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsDocsDirectory(), "examples").AsLinuxPath();
    }

    public static string AsVerbsDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsDocsDirectory(), "verbs").AsLinuxPath();
    }

    public static string AsDataDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsDocsDirectory(), "_data").AsLinuxPath();
    }

    public static string AsLinuxPath(this string path)
    {
        return path.Replace("\\", "/");
    }

    public static string AsRelativePathOf(this string directory, string rootDirectory)
    {
        return Path.GetRelativePath(rootDirectory, directory).AsLinuxPath();
    }
}
