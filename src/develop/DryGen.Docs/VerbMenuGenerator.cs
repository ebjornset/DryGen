﻿using System.IO;
using System.Linq;
using System.Text;

namespace DryGen.Docs
{
    public static class VerbMenuGenerator
    {
        public static void Generate(TextWriter writer)
        {
            var sb = new StringBuilder();
            sb.Append("# ").AppendLine(DocsConstants.AutoGeneratedNotice).AppendLine("- label: dry-gen verbs").AppendLine("  items:");
            var verbs = typeof(Generator).Assembly.GetTypes().Where(x => x.HasVerbAttribute()).Select(x => x.GetVerb());
            foreach (var verb in verbs.OrderBy(x => x))
            {
                sb.Append("  - name: ").AppendLine(verb).Append("    link: /verbs/").AppendLine(verb);
            }
            writer.Write(sb.ToString());
        }
    }
}