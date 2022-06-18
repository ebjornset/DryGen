using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryGen
{
    public static class Extensions
    {
        public static bool HasVerbAttribute(this Type type)
        {
            return type.GetCustomAttributes(typeof(VerbAttribute), inherit: true)?.Any() == true;
        }

        public static string GetVerb(this Type type)
        {
            var verbAttribute = type.CustomAttributes.Single(x => x.AttributeType == typeof(VerbAttribute));
            return verbAttribute.ConstructorArguments[0].ToString().Replace("\"", string.Empty);
        }

        public static string GetVerbHelpText(this Type type)
        {
            var verbAttribute = type.CustomAttributes.Single(x => x.AttributeType == typeof(VerbAttribute));
            return verbAttribute.NamedArguments.Single(x => x.MemberName == nameof(VerbAttribute.HelpText)).TypedValue.ToString().Replace("\"", string.Empty);
        }

        public static IReadOnlyList<OptionMetadata> GetOptionMetadataList(this Type type)
        {
            var optionList = new List<OptionMetadata>();
            foreach (var propery in type.GetProperties())
            {
                var optionAttribute = propery.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(OptionAttribute));
                if (optionAttribute == null)
                {
                    continue;
                }
                var optionMetadata = new OptionMetadata(optionAttribute);
                optionMetadata.Type = propery.PropertyType.GeneratePropertyTypeInfo(asYamlComment: false);
                optionList.Add(optionMetadata);
            }
            return optionList.OrderBy(x => x.LongName).ToArray();
        }

        public static Type GetVerbOptionsType(this string verb)
        {
            var optionsTypes = typeof(Extensions).Assembly.GetTypes().Where(type => VerbAttributeMatches(type, verb)).ToList();
            if (optionsTypes.Count == 0)
            {
                throw new ArgumentException($"Unknown verb '{verb}'", nameof(verb));
            }
            return optionsTypes.Single();
        }

        public static VerbMetadata GetVerbMetadata(this string verb)
        {
            return new VerbMetadata(verb.GetVerbOptionsType());
        }

        public static string AsMarkdownTableCellValue(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace('|', '/').Trim();
        }

        public static string GeneratePropertyTypeInfo(this Type propertyType, bool asYamlComment)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
            if (nullableUnderlyingType != null)
            {
                return GeneratePropertyTypeInfo(nullableUnderlyingType, asYamlComment);
            }
            var collectionType = GetCollectionType(propertyType);
            if (collectionType != null)
            {
                var typeInfo = GeneratePropertyTypeInfo(collectionType, asYamlComment);
                return asYamlComment? $"# List of {typeInfo}\n#- " : $"List of {typeInfo}";
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

        private static readonly Type[] collectionTypes = { typeof(IEnumerable<>) };
    }
}
