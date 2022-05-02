using System;
using System.IO;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace DryGen.CSharpFromJsonSchema
{
    public class CSharpFromJsonSchemaGenerator
    {
        public async Task<string> Generate(string? jsonSchemaFileName, JsonSchemaFileFormat jsonSchemaFileFormat)
        {
            var jsonSchema = await LoadJsonSchemaFromFile(jsonSchemaFileName, jsonSchemaFileFormat);
            RemoveSynteticSchemaProperty(jsonSchema);
            string cSharpCode = GenerateCSharpCode(jsonSchema);
            return cSharpCode;
        }

        private static string GenerateCSharpCode(JsonSchema jsonSchema)
        {
            var classGenerator = new CSharpGenerator(jsonSchema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                Namespace = "TestNameSpace", // TODO
                ArrayInstanceType = "System.Collections.Generic.List",
                GenerateOptionalPropertiesAsNullable = true,
            });
            var rootClassName = jsonSchema.Title.Replace(" ", string.Empty);// TODO
            var cSharpCode = classGenerator.GenerateFile(rootClassName);
            return cSharpCode;
        }

        private static void RemoveSynteticSchemaProperty(JsonSchema jsonSchema)
        {
            // Hack to get rid of the syntetic $schema property we must use if we want additionalProperties = false in the topmost object
            const string schemaPropertyName = "$schema";
            if (jsonSchema.Properties.ContainsKey(schemaPropertyName))
            {
                jsonSchema.Properties.Remove(schemaPropertyName);
            }
        }

        private async Task<JsonSchema> LoadJsonSchemaFromFile(string? jsonSchemaFileName, JsonSchemaFileFormat jsonSchemaFileFormat)
        {
            var extension = Path.GetExtension(jsonSchemaFileName);
            if (jsonSchemaFileFormat == JsonSchemaFileFormat.Yaml
                || (jsonSchemaFileFormat == JsonSchemaFileFormat.ByExtencion
                    && (string.Equals(".yaml", extension, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(".yml", extension, StringComparison.InvariantCultureIgnoreCase))))
            {
                return await NJsonSchema.Yaml.JsonSchemaYaml.FromFileAsync(jsonSchemaFileName);
            }
            return await JsonSchema.FromFileAsync(jsonSchemaFileName);
        }
    }
}