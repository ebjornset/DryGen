﻿using System.IO;
using System.Linq;
using System.Text;

namespace DryGen.Docs
{
    public class VerbMarkdowGenerator
    {
        public void GenerateVerbMarkdown(string verb, TextWriter writer)
        {
            var verbMetaData = verb.GetVerbMetaData();
            var sb = new StringBuilder();
            sb.AppendLine("---")
                .AppendLine(DocsConstants.AutoGeneratedNotice)
                .AppendLine("layout: page")
                .Append("title: ").AppendLine(verb)
                .Append("description: Details about the dry-gen verb ").AppendLine(verb)
                .AppendLine("show_sidebar: false")
                .AppendLine("menubar: verbs_menu")
                .AppendLine("---")
                .AppendLine(verbMetaData.HelpText);
            writer.Write(sb.ToString());
        }
    }
}
