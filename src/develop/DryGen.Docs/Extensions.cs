using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
		return Path.Combine(rootDirectory.AsTemplatesDirectory(), "releasenotes").AsLinuxPath();
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

	public static void ValidateReleaseNotesFileNames(this IEnumerable<string> releaseNotesFileNames, DateTime today)
	{
		releaseNotesFileNames = releaseNotesFileNames.Select(x => Path.GetFileName(x));
		foreach (var releaseNoteFileName in releaseNotesFileNames)
		{
			releaseNoteFileName.ValidateReleaseNotesFileName(today);
		}
	}

	public static void ValidateReleaseNotesFileName(this string releaseNotesFileName, DateTime today)
	{
		if (releaseNotesFileName == nextReleaseFileName)
		{
			return;
		}
		releaseNotesFileName.ValidateExtensionLengthAndV();
		releaseNotesFileName.ValidateReleaseDate(today);
		releaseNotesFileName.ValidateVersionNumber();
	}

	private static void ValidateVersionNumber(this string releaseNotesFileName)
	{
		var versionNumber = releaseNotesFileName.GetVersionNumber();
		var versionNumberParts = versionNumber.Split('.');
		if (versionNumberParts.Length < 3 || versionNumberParts.Length > 4)
		{
			ThrowVersionNuUmberException();
		}
		for (var i = 0; i < versionNumberParts.Length; i++)
		{
			if (versionNumberParts.Length == 4 && i == 2)
			{
				var patchParts = versionNumberParts[i].Split('-');
				if (patchParts.Length != 2)
				{
					ThrowVersionNuUmberException();
				}
				CheckInteger(patchParts[0]);
				continue;
			}
			CheckInteger(versionNumberParts[i]);
		}

		void CheckInteger(string value)
		{
			if (!int.TryParse(value, out _))
			{
				ThrowVersionNuUmberException();
			}
		}

		void ThrowVersionNuUmberException()
		{
			throw new FileNameException(releaseNotesFileName, $"'{versionNumber}' is not a valid version number. {validVersionNumberText}");
		}
	}

	private static void ValidateExtensionLengthAndV(this string releaseNotesFileName)
	{
		if (Path.GetExtension(releaseNotesFileName) != ".md" || releaseNotesFileName.Length < nextReleaseFileNameLength)
		{
			throw new FileNameException(releaseNotesFileName, validFileNameText);
		}
		if (releaseNotesFileName[10..13] != "-v-")
		{
			throw new FileNameException(releaseNotesFileName, validFileNameText);
		}
	}

	private static void ValidateReleaseDate(this string releaseNotesFileName, DateTime today)
	{
		if (!DateTime.TryParseExact(releaseNotesFileName.GetReleaseDate(), nextReleaseFileNamePrefix, CultureInfo.InvariantCulture, DateTimeStyles.None, out var releaseDate))
		{
			throw new FileNameException(releaseNotesFileName, validFileNameText);
		}
		if (releaseDate > today.Date)
		{
			throw new FileNameException(releaseNotesFileName, $"'{releaseDate.ToString(nextReleaseFileNamePrefix)}' is in the future");
		}
	}

	public static string GetReleaseDate(this string releaseNotesFileName)
	{
		return releaseNotesFileName[0..10];
	}

	public static string GetVersionNumber(this string releaseNotesFileName)
	{
		return Path.GetFileNameWithoutExtension(releaseNotesFileName[13..]);
	}

	public static string GetOutputFileName(this string releaseNotesFileName)
	{
		return releaseNotesFileName[11..];
	}

	private const string nextReleaseFileNamePrefix = "yyyy-MM-dd";
	private const string nextReleaseFileName = "yyyy-MM-dd-v-x.y.z.md";
	private const string validFileNameText = "The file name must be 'yyyy-MM-dd-v-x.y.z.md' or on the format '<yyyy-MM-dd>-v-<x.y.z>.md', where <yyyy-MM-dd> is a date (NB! Not in the future) and <x.y.z> is a valid version number!";
	private const string validVersionNumberText = "It must be on the format a[.b[.c[-<prerelease name>.d]]], where a, b, c and d are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease'";
	private static readonly int nextReleaseFileNameLength = nextReleaseFileName.Length;
}
