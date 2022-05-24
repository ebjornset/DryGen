using CommandLine;
using DryGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace DryGen
{
    public class OptionsFromCommandlineGenerator
    {
        public string Generate(OptionsFromCommandlineOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Verb))
            {
                throw new RequiredOptionMissingException(nameof(options.Verb));
            }
            var optionsTypeFromVerb = GetOptionsTypeFromVerb(options.Verb);
            var optionsYaml = GetOptionsYaml(optionsTypeFromVerb, options.Verb);
            return optionsYaml;
        }

        private Type GetOptionsTypeFromVerb(string verb)
        {
            var optionsTypes = GetType().Assembly.GetTypes().Where(type => VerbAttributeMatches(type, verb)).ToList();
            if (optionsTypes.Count == 0)
            {
                throw new ArgumentException($"Unknown verb '{verb}'", nameof(verb));
            }
            return optionsTypes.Single();
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
            foreach (var propery in optionsTypeFromVerb.GetProperties())
            {
                var yamlMemberAttribute = propery.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(YamlMemberAttribute));
                var alias = yamlMemberAttribute?.NamedArguments.Any(x => x.MemberName == nameof(YamlMemberAttribute.Alias)) == true
                    ? yamlMemberAttribute?.NamedArguments.Single(x => x.MemberName == nameof(YamlMemberAttribute.Alias)).TypedValue.ToString().Replace("\"", string.Empty)
                    : null;
                if (string.IsNullOrEmpty(alias))
                {
                    continue;
                }
                var propertyTypeInfo = GeneratePropertyTypeInfo(propery.PropertyType);
                optionList.Add($"#{alias}: {propertyTypeInfo}");
            }

            return optionList;
        }

        private static string GeneratePropertyTypeInfo(Type propertyType)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
            if (nullableUnderlyingType != null)
            {
                return GeneratePropertyTypeInfo(nullableUnderlyingType);
            }
            var collectionType = GetCollectionType(propertyType);
            if (collectionType != null)
            {
                var typeInfo = GeneratePropertyTypeInfo(collectionType);
                return $"# List of {typeInfo}\n#- ";
            }
            if (propertyType.IsEnum)
            {
                return string.Join(" | ", Enum.GetNames(propertyType).Select(x => x.ToLowerInvariant()));
            }
            if (propertyType == typeof(bool))
            {
                return "true|false";
            }
            return propertyType.Name.ToLowerInvariant();
        }

        private static Type? GetCollectionType(Type type)
        {
            if (type.IsGenericType
                && collectionTypes.Contains(type.GetGenericTypeDefinition())
                && type.GetGenericArguments().Length == 1)
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        private static bool VerbAttributeMatches(Type type, string verb)
        {
            if (type.GetCustomAttributes(typeof(VerbAttribute), inherit: true)?.Any() == true)
            {
                var verbAttribute = type.CustomAttributes.Single(x => x.AttributeType == typeof(VerbAttribute));
                var optionsVerb = verbAttribute.ConstructorArguments[0].ToString()?.Replace("\"", string.Empty);
                if (string.Equals(optionsVerb, verb, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static readonly Type[] collectionTypes = { typeof(IEnumerable<>) };
    }
}
