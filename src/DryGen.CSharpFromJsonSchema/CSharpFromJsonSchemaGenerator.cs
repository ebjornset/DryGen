﻿using DryGen.Core;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DryGen.CSharpFromJsonSchema;

public static class CSharpFromJsonSchemaGenerator
{
    public static async Task<string> Generate(ICSharpFromJsonSchemaOptions options)
    {
        var jsonSchema = await LoadJsonSchemaFromFile(options.InputFile, options.GetSchemaFileFormat());
        RemoveSynteticSchemaProperty(jsonSchema);
        string cSharpCode = GenerateCSharpCode(jsonSchema, options.Namespace, options.RootClassname, options.ArrayType, options.ArrayInstanceType);
        return cSharpCode;
    }

    private static string GenerateCSharpCode(JsonSchema jsonSchema, string? theNamespace, string? rootClassname, string? arrayType, string? arrayInstanceType)
    {
        var settings = new CSharpGeneratorSettings
        {
            ClassStyle = CSharpClassStyle.Poco,
            Namespace = GetNamespace(theNamespace),
            GenerateOptionalPropertiesAsNullable = true,
        };
        if (!string.IsNullOrWhiteSpace(arrayType))
        {
            settings.ArrayType = arrayType;
        }
        if (!string.IsNullOrWhiteSpace(arrayInstanceType))
        {
            settings.ArrayInstanceType = arrayInstanceType;
        }
        var classGenerator = new CSharpGenerator(jsonSchema, settings);
        var cSharpCode = classGenerator.GenerateFile(GetRootClassName(jsonSchema, rootClassname));
        return cSharpCode;

        static string GetNamespace(string? theNamespace)
        {
            return string.IsNullOrWhiteSpace(theNamespace) ? "CSharpFromJsonSchema" : theNamespace;
        }

        static string GetRootClassName(JsonSchema jsonSchema, string? rootClassname)
        {
            if (!string.IsNullOrWhiteSpace(rootClassname))
            {
                return rootClassname;
            }
            var schemaTitle = jsonSchema.Title?.Replace(" ", string.Empty);
            return string.IsNullOrWhiteSpace(schemaTitle) ? "ClassFromJsonSchema" : schemaTitle;
        }
    }

    private static void RemoveSynteticSchemaProperty(JsonSchema jsonSchema)
    {
        // Hack to get rid of the syntetic $schema property we must use if we want additionalProperties = false in the topmost object
        const string schemaPropertyName = "$schema";
        jsonSchema.Properties.Remove(schemaPropertyName);
    }

    private static async Task<JsonSchema> LoadJsonSchemaFromFile(string? jsonSchemaFileName, JsonSchemaFileFormat jsonSchemaFileFormat)
    {
        jsonSchemaFileName = jsonSchemaFileName.AsNonNull();
        var extension = Path.GetExtension(jsonSchemaFileName);
        if (jsonSchemaFileFormat == JsonSchemaFileFormat.Yaml
            || (jsonSchemaFileFormat == JsonSchemaFileFormat.ByExtension
                && (string.Equals(".yaml", extension, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(".yml", extension, StringComparison.InvariantCultureIgnoreCase))))
        {
            return await NJsonSchema.Yaml.JsonSchemaYaml.FromFileAsync(jsonSchemaFileName);
        }
        return await JsonSchema.FromFileAsync(jsonSchemaFileName);
    }
}