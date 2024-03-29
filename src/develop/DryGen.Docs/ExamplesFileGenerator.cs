﻿using System.IO;

namespace DryGen.Docs;

public static class ExamplesFileGenerator
{
    public static void Generate(string rootDirectory, string fileName)
    {
        var sourceFile = Path.Combine(rootDirectory.AsExamplesTemplatesDirectory().AsRelativePathOf(rootDirectory), fileName).AsLinuxPath();
        var fileContent = File.ReadAllText(Path.Combine(rootDirectory.AsExamplesTemplatesDirectory(), fileName))
            .Replace(DocsConstants.ReplaceTokenForAutoGeneratedByDocsWarning, DocsConstants.AutoGeneratedNotice)
            .Replace(DocsConstants.ReplaceTokenForAutoGeneratedByDocsSource, $"Source file: \"{sourceFile}\"");
        File.WriteAllText(Path.Combine(rootDirectory.AsExamplesDirectory(), fileName.ToLowerInvariant()), fileContent);
    }
}