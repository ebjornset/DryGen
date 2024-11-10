﻿using System;
using System.IO;
using DryGen.Core;

namespace DryGen.Docs;

public static class ReleaseNotesFileGenerator
{
	public static void Generate(string rootDirectory, string fileName, DateTime today)
	{
		fileName.ValidateReleaseNotesFileName(today);
		var sourceFile = Path.Combine(rootDirectory.AsTemplatesReleaseNotesDirectory().AsRelativePathOf(rootDirectory), fileName).AsLinuxPath();
		var fileContent = File.ReadAllText(Path.Combine(rootDirectory.AsTemplatesReleaseNotesDirectory(), fileName))
			.Replace(DocsConstants.ReplaceTokenForReleaseNotestHeader, GetReleaseNotestHeader());
		File.WriteAllText(Path.Combine(rootDirectory.AsGeneratedReleaseNotesDirectoryCreated(), fileName.GetOutputFileName()), fileContent);

		string GetReleaseNotestHeader()
		{
			var releaseDate = fileName.GetReleaseDate();
			var versionNumber = fileName.GetVersionNumber();
			return $"""
				<!--
				{DocsConstants.AutoGeneratedNotice}
				Source file: "{sourceFile}"
				-->
				# Version {versionNumber}
				**Release date {releaseDate}**

				""";
		}
	}
}