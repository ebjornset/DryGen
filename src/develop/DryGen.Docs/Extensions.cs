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
        return Path.Combine(rootDirectory.AsDocsDirectory(), "templates").AsLinuxPath();
    }

    public static string AsGeneratedDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsDocsDirectory(), "_generated").AsLinuxPath();
    }

	public static string AsGeneratedIncludeDirectory(this string rootDirectory)
	{
		return Path.Combine(rootDirectory.AsGeneratedDirectory(), "_include").AsLinuxPath();
	}

	public static string AsTemplatesExamplesDirectory(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsTemplatesDirectory(), "examples").AsLinuxPath();
    }

	public static string AsTemplatesReleaseNotesDirectory(this string rootDirectory)
	{
		return Path.Combine(rootDirectory.AsTemplatesDirectory(), "releases").AsLinuxPath();
	}

	public static string AsGeneratedExamplesDirectoryCreated(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsGeneratedDirectory(), "examples").AsLinuxPath().CreateDirectories();
    }

	public static string AsGeneratedIncludeExamplesDirectoryCreated(this string rootDirectory)
	{
		return Path.Combine(rootDirectory.AsGeneratedIncludeDirectory(), "examples").AsLinuxPath().CreateDirectories();
	}

	public static string AsGeneratedReleaseNotesDirectoryCreated(this string rootDirectory)
	{
		return Path.Combine(rootDirectory.AsGeneratedDirectory(), "releases").AsLinuxPath().CreateDirectories();
	}

	public static string AsGeneratedVerbsDirectoryCreated(this string rootDirectory)
    {
        return Path.Combine(rootDirectory.AsGeneratedDirectory(), "verbs").AsLinuxPath().CreateDirectories();
    }

    public static string AsLinuxPath(this string path)
    {
        return path.Replace("\\", "/");
    }

    public static string AsRelativePathOf(this string directory, string rootDirectory)
    {
        return Path.GetRelativePath(rootDirectory, directory).AsLinuxPath();
    }

    public static string CreateDirectories(this string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}