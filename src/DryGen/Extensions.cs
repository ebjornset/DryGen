using CommandLine;
using DryGen.Options;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace DryGen;

public static class Extensions
{
    public static bool HasVerbAttribute(this Type type)
    {
        return type.GetCustomAttributes(typeof(VerbAttribute), inherit: true)?.Length > 0;
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
        foreach (var property in type.GetProperties())
        {
            var optionAttribute = property.GetVisibleOptionAttribute();
            if (optionAttribute == null)
            {
                continue;
            }
            var optionMetadata = new OptionMetadata(optionAttribute)
            {
                Type = property.PropertyType.GeneratePropertyTypeInfo(asYamlComment: false)
            };
            optionList.Add(optionMetadata);
        }
        return optionList.OrderBy(x => x.LongName).ToArray();
    }

    public static CustomAttributeData? GetVisibleOptionAttribute(this PropertyInfo property)
    {
        var optionAttribute = property.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(OptionAttribute));
        // Remove Hidden Option
        if (optionAttribute == null ||
            (optionAttribute.NamedArguments.Any(x => x.MemberName == nameof(OptionAttribute.Hidden)) &&
                string.Equals(
                    bool.TrueString,
                    optionAttribute.NamedArguments.Single(x => x.MemberName == nameof(OptionAttribute.Hidden)).TypedValue.Value?.ToString(),
                    StringComparison.InvariantCultureIgnoreCase)))
        {
            return null;
        }
        return optionAttribute;
    }

    public static Type GetVerbOptionsType(this string verb)
    {
        var optionsTypes = typeof(Extensions).Assembly.GetTypes().Where(type => VerbAttributeMatches(type, verb)).ToList();
        if (optionsTypes.Count == 0)
        {
            throw new OptionsException($"Unknown verb '{nameof(verb)}'");
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

    public static string GeneratePropertyTypeInfo(this Type propertyType, bool asYamlComment, string listValueIndention = "")
    {
        var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
        if (nullableUnderlyingType != null)
        {
            return nullableUnderlyingType.GeneratePropertyTypeInfo(asYamlComment, listValueIndention);
        }
        var collectionType = GetCollectionType(propertyType);
        if (collectionType != null)
        {
            var typeInfo = collectionType.GeneratePropertyTypeInfo(asYamlComment, listValueIndention);
            return asYamlComment ? $"# List of {typeInfo}\n{listValueIndention}#- " : $"List of {typeInfo}";
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

    public static TOptions AsNonNullOptions<TOptions>(this CommonOptions? options) where TOptions : BaseOptions
    {
        return options as TOptions ?? throw new ArgumentException($"Cannot cast options '{options}' as '{typeof(TOptions)}'", nameof(options));
    }

    public static T AsNonNull<T>(this T? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        return value;
    }

    public static string ReadOptionsFileWithEnviromentVariableReplacement(this string optionsFile)
    {
        var startIndex = 0;
        var yaml = File.ReadAllText(optionsFile);
        do
        {
            startIndex = yaml.IndexOf("$(", startIndex);
            if (startIndex == -1)
            {
                break;
            }
            var endIndex = yaml.IndexOf(')', startIndex);
            if (endIndex <= startIndex)
            {
                break;
            }
            var variable = yaml[startIndex..(endIndex + 1)];
            var environmentVariable = variable.Replace("$(", string.Empty).Replace(")", string.Empty).Trim();
            var value = Environment.GetEnvironmentVariable(environmentVariable) ?? string.Empty;
            yaml = yaml.Replace(variable, value);
        }
        while (true);
        return yaml;
    }

    public static IEnumerable<PropertyInfo> GetYamlMemberProperties(this Type type, bool excludeDeprecated)
    {
        var properties = new List<PropertyInfo>();
        foreach (var property in type.GetProperties())
        {
            var alias = property.GetYamlMemberAttributeAlias();
            if (string.IsNullOrEmpty(alias))
            {
                continue;
            }
            var optionAttribute = property.GetVisibleOptionAttribute();
            if (optionAttribute == null)
            {
                continue;
            }
            if (excludeDeprecated)
            {
                var optionMetadata = new OptionMetadata(optionAttribute);
                if (optionMetadata.Description?.StartsWith(Constants.DeprecatedNotice) == true)
                {
                    continue;
                }
            }
            property.ValidateNullable(type);
            properties.Add(property);
        }
        return properties;
    }

    [ExcludeFromCodeCoverage(Justification = "Sanity check that we dont start using non nullable types as yaml members")]
    public static void ValidateNullable(this PropertyInfo property, Type type)
    {
        if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
        {
            throw new OptionsException($"Non nullable yaml member property '{property.Name}' found in type '{type.FullName}");
        }
    }

    public static string? GetYamlMemberAttributeAlias(this PropertyInfo property)
    {
        var yamlMemberAttribute = property.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(YamlMemberAttribute));
        var alias = yamlMemberAttribute?.NamedArguments.Any(x => x.MemberName == nameof(YamlMemberAttribute.Alias)) == true
            ? yamlMemberAttribute.NamedArguments.Single(x => x.MemberName == nameof(YamlMemberAttribute.Alias)).TypedValue.ToString().Replace("\"", string.Empty)
            : null;
        return alias;
    }

    private static bool VerbAttributeMatches(Type type, string verb)
    {
        if (type.GetCustomAttributes(typeof(VerbAttribute), inherit: true)?.Length > 0)
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