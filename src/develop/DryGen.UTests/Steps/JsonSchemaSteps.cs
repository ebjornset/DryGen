using DryGen.CSharpFromJsonSchema;
using DryGen.DevUtils.Helpers;
using DryGen.Options;
using System;
using System.IO;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public class JsonSchemaSteps
    {
        private string? jsonSchemaText;
        private readonly ExceptionContext exceptionContext;
        private readonly InputFileContext inputFileContext;
        private readonly GeneratedRepresentationContext generatedRepresentationContext;

        public JsonSchemaSteps(
            ExceptionContext exceptionContext,
            InputFileContext inputFileContext,
            GeneratedRepresentationContext generatedRepresentationContext)
        {
            this.exceptionContext = exceptionContext;
            this.inputFileContext = inputFileContext;
            this.generatedRepresentationContext = generatedRepresentationContext;
        }

        [Given(@"this json schema")]
        public void GivenThisJsonSchema(string multilineText)
        {
            jsonSchemaText = multilineText;
        }

        [Given(@"this json schema input file with the extension ""([^""]*)""")]
        public void GivenThisJsonSchemaInputFileWithTheExtension(string extension, string multilineText)
        {
            GivenThisJsonSchema(multilineText);
            CreateTestJsonFile(extension);
        }

        [When(@"I load the json schema from a file with the extension ""([^""]*)""")]
        public void WhenILoadTheJsonSchemaFromAFileWithTheExtension(string extension)
        {
            CreateAndLoadTestJsonFile(extension, JsonSchemaFileFormat.ByExtension);
        }

        [When(@"I load the json schema from a file forcing the schema format ""([^""]*)""")]
        public void WhenILoadTheJsonSchemaFromAFileForcingTheSchemaFormat(string forcedSchemaFormat)
        {
            var jsonSchemaFileFormat = (JsonSchemaFileFormat)Enum.Parse(typeof(JsonSchemaFileFormat), forcedSchemaFormat, true);
            CreateAndLoadTestJsonFile("forced", jsonSchemaFileFormat);
        }

        private void CreateAndLoadTestJsonFile(string extension, JsonSchemaFileFormat jsonSchemaFileFormat)
        {
            CreateTestJsonFile(extension);
            LoadJsonSchemaFromFile(jsonSchemaFileFormat);
        }

        private void CreateTestJsonFile(string extension)
        {
            if (string.IsNullOrWhiteSpace(jsonSchemaText))
            {
                throw new ArgumentException("Json schema is not specified");
            }
            string jsonSchemaFileName;
            do
            {
                var tmpFileName = Path.GetTempFileName();
                jsonSchemaFileName = Path.ChangeExtension(tmpFileName, extension);
                if (File.Exists(tmpFileName))
                {
                    File.Delete(tmpFileName);
                }
            }
            while (File.Exists(jsonSchemaFileName));
            File.WriteAllText(jsonSchemaFileName, jsonSchemaText);
            inputFileContext.InputFileName = jsonSchemaFileName;
        }

        private void LoadJsonSchemaFromFile(JsonSchemaFileFormat jsonSchemaFileFormat)
        {
            var generator = new CSharpFromJsonSchemaGenerator();
            var options = new CSharpFromJsonSchemaOptions
            {
                InputFile = inputFileContext.InputFileName,
                SchemaFileFormat = jsonSchemaFileFormat
            };
            generatedRepresentationContext.GeneratedRepresentation = exceptionContext.HarvestExceptionFrom(() => generator.Generate(options).Result);
        }
    }
}
