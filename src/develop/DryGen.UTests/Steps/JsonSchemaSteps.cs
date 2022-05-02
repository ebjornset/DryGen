using DryGen.CSharpFromJsonSchema;
using DryGen.DevUtils.Helpers;
using DryGen.UTests.Helpers;
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
            CreateAndLoadTestJsonFile(extension, JsonSchemaFileFormat.ByExtencion);
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
            var jsonSchemaFileName = Path.ChangeExtension(Path.GetTempFileName(), extension);
            if (File.Exists(jsonSchemaFileName))
            {
                throw new ArgumentException($"Json schema file '{jsonSchemaFileName}' already exists");
            }
            File.WriteAllText(jsonSchemaFileName, jsonSchemaText);
            inputFileContext.InputFileName = jsonSchemaFileName;
        }

        private void LoadJsonSchemaFromFile(JsonSchemaFileFormat jsonSchemaFileFormat)
        {
            var generator = new CSharpFromJsonSchemaGenerator();
            generatedRepresentationContext.GeneratedRepresentation = exceptionContext.HarvestExceptionFrom(() =>
               generator.Generate(inputFileContext.InputFileName, jsonSchemaFileFormat).Result);
        }
    }
}
