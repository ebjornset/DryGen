using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DryGen.Core;

public static class Extensions
{
    public static Type LoadTypeByName(this string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        throw new TypeLoadException($"Could not load type '{typeName}'");
    }

    public static Regex ToSingleLineCompiledRegexWithTimeout(this string regex)
    {
        return new Regex(regex, RegexOptions.Singleline | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }

    public static bool IsKeyAttribute(this CustomAttributeData attributeData)
    {
        return attributeData.AttributeType.FullName == "System.ComponentModel.DataAnnotations.KeyAttribute";
    }

    public static bool IsRequiredAttribute(this CustomAttributeData attributeData)
    {
        return attributeData.AttributeType.FullName == "System.ComponentModel.DataAnnotations.RequiredAttribute";
    }

    public static bool IsJsonPropertyRequiredAttribute(this CustomAttributeData attributeData)
    {
        if (attributeData.AttributeType.FullName != "Newtonsoft.Json.JsonPropertyAttribute")
        {
            return false;
        }
        var requiredArgumentValue = attributeData.NamedArguments.Any(x => x.MemberName == "Required")
            ? attributeData.NamedArguments.Single(x => x.MemberName == "Required").TypedValue.Value
            : null;
        // 2 is the enum value of "Newtonsoft.Json.Required.Always"
        // NB! This should maybe be made more fool proof?
        return requiredArgumentValue is int requiredArgumentIntValue && requiredArgumentIntValue == 2;
    }

    public static bool IsRequiredProperty(this PropertyInfo propertyInfo)
    {
        return propertyInfo.CustomAttributes.Any(x => x.IsRequiredAttribute() || x.IsJsonPropertyRequiredAttribute());
    }

    public static string GetRandomFileName(this string folder, bool stripExtesion = false)
    {
        while (true)
        {
            var fileName = Path.GetRandomFileName();
            if (stripExtesion)
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }
            var result = Path.Combine(folder, fileName);
            if (File.Exists(result))
            {
                continue;
            }
            return result;
        }
    }
}