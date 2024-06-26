﻿using System.IO;
using System.Linq;
using System.Text;

namespace DryGen.Docs;

public static class ExamplesTocGenerator
{
	public static void Generate(TextWriter writer, string examplesTemplatesDirectory)
	{
		var sb = new StringBuilder().Append("# ").AppendLine(DocsConstants.AutoGeneratedNotice).AppendLine("- name: Overview").AppendLine("  href: index.md");
		foreach (var exampleTemplateFile in Directory.GetFiles(examplesTemplatesDirectory).OrderBy(x => x))
		{
			var fileName = Path.GetFileNameWithoutExtension(exampleTemplateFile);
			sb.Append("- name: ").AppendLine(fileName.Replace('_', ' ').Replace('-', ' '))
				.Append("  href: ").Append(fileName.ToLowerInvariant()).AppendLine(".md");
		}
		writer.Write(sb.ToString());
	}
}
