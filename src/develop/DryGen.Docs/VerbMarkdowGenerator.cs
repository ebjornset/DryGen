﻿using System.IO;
using System.Text;

namespace DryGen.Docs;

public static class VerbMarkdowGenerator
{
    public static void Generate(string verb, TextWriter writer)
    {
        var verbMetaData = verb.GetVerbMetadata();
        var sb = new StringBuilder();
        sb.AppendLine("---")
            .Append("# ").AppendLine(DocsConstants.AutoGeneratedNotice)
            .AppendLine("layout: page")
            .Append("title: ").AppendLine(verb)
            .Append("description: Details about the dry-gen verb ").AppendLine(verb)
            .AppendLine("show_sidebar: false")
            .AppendLine("menubar: verbs_menu")
            .AppendLine("hero_height: is-fullwidth")
            .AppendLine("---")
            .Append(verbMetaData.HelpText).AppendLine(" ").AppendLine()
            .AppendLine("## Options")
            .Append("The verb '").Append(verb).AppendLine("' uses the following options.")
            .AppendLine()
            .AppendLine("|Option|Alias|Type|Description|")
            .AppendLine("|---|---|---|---|");
        foreach (var optionMetadata in verbMetaData.Options)
        {
            sb.Append('|')
              .Append(optionMetadata.LongName.AsMarkdownTableCellValue()).Append('|')
              .Append(optionMetadata.ShortName.AsMarkdownTableCellValue()).Append('|')
              .Append(optionMetadata.Type.AsMarkdownTableCellValue()).Append('|')
              .Append(optionMetadata.Description.AsMarkdownTableCellValue()).AppendLine("|");
        }
        sb.AppendLine()
          .AppendLine("{% include notification.html status=\"is-dark\" ")
          .AppendLine("message=\"You can always get information about this verb's options by running the command ")
          .AppendLine()
          .Append("`dry-gen ").Append(verb).AppendLine(" --help`.\"")
          .AppendLine("%}")
          .AppendLine("## Options file template")
          .Append("Here is a template for an options file for '").Append(verb).AppendLine("'. ")
          .AppendLine("```");
        using var optionTemplateWriter = new StringWriter();
        new Generator(optionTemplateWriter, optionTemplateWriter).Run(new[] { "options-from-commandline", "--verb", verb });
        sb.AppendLine(optionTemplateWriter.ToString())
          .AppendLine("```")
          .AppendLine("{% include notification.html status=\"is-dark\" ")
          .AppendLine("message=\"You can generate the same template your self with the command ")
          .AppendLine()
          .Append("`dry-gen options-from-commandline --verb ").Append(verb).AppendLine("`\"")
          .AppendLine("%}");
        writer.Write(sb.ToString());
    }
}