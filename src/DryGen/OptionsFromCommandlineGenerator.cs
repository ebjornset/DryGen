using DryGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace DryGen
{
    public static class OptionsFromCommandlineGenerator
    {
        public static string Generate(OptionsFromCommandlineOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Verb))
            {
                throw new OptionsException($"option {nameof(options.Verb)} is required");
            }
            var optionsTypeFromVerb = options.Verb.GetVerbOptionsType();
            var optionsYaml = GetOptionsYaml(optionsTypeFromVerb, options.Verb);
            return optionsYaml;
        }

        private static string GetOptionsYaml(Type optionsTypeFromVerb, string verb)
        {
            List<string> optionList = GenerateOptionsList(optionsTypeFromVerb);
            var sb = new StringBuilder().AppendLine("#").Append("# dry-gen options for verb '").Append(verb).AppendLine("'").AppendLine("#").AppendJoin('\n', optionList.OrderBy(x => x));
            return sb.ToString();
        }

        private static List<string> GenerateOptionsList(Type optionsTypeFromVerb)
        {
            var optionList = new List<string>();
            foreach (var property in optionsTypeFromVerb.GetProperties())
            {
                var yamlMemberAttribute = property.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(YamlMemberAttribute));
                var alias = yamlMemberAttribute?.NamedArguments.Any(x => x.MemberName == nameof(YamlMemberAttribute.Alias)) == true
                    ? yamlMemberAttribute.NamedArguments.Single(x => x.MemberName == nameof(YamlMemberAttribute.Alias)).TypedValue.ToString().Replace("\"", string.Empty)
                    : null;
                if (string.IsNullOrEmpty(alias))
                {
                    continue;
                }
                var optionAttribute = property.GetVisibleOptionAttribute();
                if (optionAttribute == null)
                {
                    continue;
                }
                var optionMetadata = new OptionMetadata(optionAttribute);
                if (optionMetadata.Description?.StartsWith(Constants.DeprecatedNotice) == true)
                {
                    continue;
                }
                var propertyTypeInfo = property.PropertyType.GeneratePropertyTypeInfo(asYamlComment: true);
                optionList.Add($"#{alias}: {propertyTypeInfo}");
            }
            return optionList;
        }
    }
}
