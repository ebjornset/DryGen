using CommandLine;
using System;
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
            CustomAttributeData verbAttribute = type.CustomAttributes.Single(x => x.AttributeType == typeof(VerbAttribute));
            return verbAttribute.NamedArguments.Single(x => x.MemberName == nameof(VerbAttribute.HelpText)).TypedValue.ToString().Replace("\"", string.Empty);
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

        public static VerbMetaData GetVerbMetaData(this string verb)
        {
            return new VerbMetaData(verb.GetVerbOptionsType());
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
    }
}
