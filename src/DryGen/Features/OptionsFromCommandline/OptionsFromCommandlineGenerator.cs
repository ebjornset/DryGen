using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace DryGen.Features.OptionsFromCommandline
{
    public static class OptionsFromCommandlineGenerator
    {
        public static string Generate(OptionsFromCommandlineOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Verb))
            {
                throw new OptionsException($"option {nameof(options.Verb)} is required");
            }
            var sb = new StringBuilder();
            sb = sb.AppendVerbHeader(options.Verb);
            if (options.Verb == Constants.VerbsFromOptionsFile.Verb)
            {
                sb.AppendLine("# There is one yaml document for each supported verb below. Uncomment the ones you need and delete the rest.").AppendLine("#");
                var verbs = new[] {
                    Constants.CsharpFromJsonSchema.Verb,
                    Constants.MermaidClassDiagramFromCsharp.Verb,
                    Constants.MermaidClassDiagramFromJsonSchema.Verb,
                    Constants.MermaidErDiagramFromCsharp.Verb,
                    Constants.MermaidErDiagramFromEfCore.Verb,
                    Constants.MermaidErDiagramFromJsonSchema.Verb,
                    Constants.OptionsFromCommandline.Verb,
                }.OrderBy(x => x);
                sb = AppendOptionsYamlFromVerbs(sb, verbs, "    ");
            }
            else
            {
                sb = AppendOptionsYamlFromVerb(sb, options.Verb, string.Empty);
            }
            return sb.ToString();
        }

        private static StringBuilder AppendOptionsYamlFromVerbs(StringBuilder sb, IEnumerable<string> verbs, string indention)
        {
            var first = true;
            foreach (var verb in verbs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine().AppendLine("#---");
                }
                sb.AppendLine("#configuration:").Append("  #verb: ").AppendLine(verb)
                    .AppendLine("  #name: string #optional, must be unique among the named yaml documents in this file if it's provided.")
                    .AppendLine("  #inhert-options-from: string #optional, name of another yaml document with the same verb in this file.")
                    .AppendLine("  #options:");
                sb = AppendOptionsYamlFromVerb(sb, verb, indention);
            }
            return sb;
        }

        private static StringBuilder AppendVerbHeader(this StringBuilder sb, string verb)
        {
            return sb.AppendLine("#").Append("# dry-gen options for verb '").Append(verb).AppendLine("'").AppendLine("#");
        }

        private static StringBuilder AppendOptionsYamlFromVerb(StringBuilder sb, string verb, string indention)
        {
            var optionsTypeFromVerb = verb.GetVerbOptionsType();
            sb = sb.AppendOptionsYamlFromType(optionsTypeFromVerb, indention);
            return sb;
        }

        private static StringBuilder AppendOptionsYamlFromType(this StringBuilder sb, Type optionsTypeFromVerb, string indention)
        {
            List<string> optionList = GenerateOptionsList(optionsTypeFromVerb, indention);
            return sb.AppendJoin('\n', optionList.OrderBy(x => x));
        }

        private static List<string> GenerateOptionsList(Type optionsTypeFromVerb, string indention)
        {
            var optionList = new List<string>();
            var yamleMemberProperties = optionsTypeFromVerb.GetYamlMemberProperties(excludeDeprecated: true);
            foreach (var property in yamleMemberProperties)
            {
                var alias = property.GetYamlMemberAttributeAlias().AsNonNull();
                var propertyTypeInfo = property.PropertyType.GeneratePropertyTypeInfo(asYamlComment: true, indention);
                optionList.Add($"{indention}#{alias}: {propertyTypeInfo}");
            }
            return optionList;
        }
    }
}
