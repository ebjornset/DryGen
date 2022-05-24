using CommandLine;
using CommandLine.Text;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.MermaidFromCSharp.ErDiagram;
using DryGen.MermaidFromCSharp.NameRewriters;
using DryGen.MermaidFromCSharp.PropertyFilters;
using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using DryGen.CSharpFromJsonSchema;
using DryGen.Options;
using DryGen.MermaidFromJsonSchema;

namespace DryGen
{
    public class Generator
    {
        private readonly Parser parser;
        private readonly TextWriter outWriter;
        private readonly TextWriter errorWriter;

        public Generator(TextWriter outWriter, TextWriter errorWriter)
        {
            parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
            });
            this.outWriter = outWriter;
            this.errorWriter = errorWriter;
        }

        public int Run(string[] args)
        {
            var parserResult = parser.ParseArguments<
                CSharpFromJsonSchemaOptions,
                MermaidClassDiagramFromCSharpOptions,
                MermaidClassDiagramFromJsonSchemaOptions,
                MermaidErDiagramFromCSharpOptions,
                MermaidErDiagramFromEfCoreOptions,
                MermaidErDiagramFromJsonSchemaOptions,
                OptionsFromCommandlineOptions
                 >(args);
            return parserResult.MapResult(
              (CSharpFromJsonSchemaOptions options) => GenerateCSharpFromJsonSchema(options, args),
              (MermaidClassDiagramFromCSharpOptions options) => GenerateMermaidClassDiagramFropmCSharp(options, args),
              (MermaidClassDiagramFromJsonSchemaOptions options) => GenerateMermaidClassDiagramFromJsonSchema(options, args),
              (MermaidErDiagramFromCSharpOptions options) => GenerateMermaidErDiagramFromCSharp(options, args),
              (MermaidErDiagramFromEfCoreOptions options) => GenerateMermaidErDiagramFromEfCore(options, args),
              (MermaidErDiagramFromJsonSchemaOptions options) => GenerateMermaidErDiagramFromJsonSchema(options, args),
              (OptionsFromCommandlineOptions options) => GenerateOptionsFromCommandline(options, args),
              errors => DisplayHelp(parserResult));
        }

        private int DisplayHelp<T>(ParserResult<T> result)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                result.WithNotParsed(errors =>
                {
                    if (errors.Any(x => x.Tag == ErrorType.BadVerbSelectedError))
                    {
                        h.AddPostOptionsText("Rerun the command with a valid verb and --help to get more help information about the spesific verb.");
                    }
                });
                h.AutoHelp = false;     // hides --help
                h.AutoVersion = false;  // hides --version
                var helpText = HelpText.DefaultParsingErrorsHandler(result, h);
                return helpText;
            }, e => e, verbsIndex: true);
            errorWriter.WriteLine(helpText);
            return 1;
        }

        private int ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay<TOptions>(TOptions options, string[] args, string resultRepresentation, Func<TOptions, string> resultFunc) where TOptions : BaseOptions, new()
        {
            return ExecuteWithExceptionHandlingAndHelpDisplay(options, options =>
            {
                options = GetOptionsFromFileWithCommandlineOptionsAsOverrides(options, args);
                if (!string.IsNullOrEmpty(options.OutputFile))
                {
                    outWriter.WriteLine($"Generating {resultRepresentation} to file '{options.OutputFile}'");
                }
                var resultReprersentation = resultFunc(options);
                WriteGeneratedRepresentationToConsoleOrFile(options, resultReprersentation);
                return 0;
            });
        }

        private int ExecuteWithExceptionHandlingAndHelpDisplay<TOptions>(TOptions options, Func<TOptions, int> verbFunc) where TOptions : BaseOptions, new()
        {
            try
            {
                return verbFunc(options);
            }
            catch (Exception ex)
            {
                var verbAttribute = options.GetType().GetCustomAttribute<VerbAttribute>();
                if (verbAttribute != null)
                {
                    errorWriter.WriteLine($"VERB: {verbAttribute.Name} ({verbAttribute.HelpText})");
                }
                errorWriter.WriteLine();
                errorWriter.WriteLine($"ERROR: {ex.Message}");
                errorWriter.WriteLine($"Rerun the command with --help to get more help information");
                return 1;
            }
        }

        private int GenerateMermaidErDiagramFromCSharp(MermaidErDiagramFromCSharpOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid ER diagram", options =>
            {
                var diagramGenerator = new ErDiagramGenerator(options);
                return GenerateMermaidDiagramFromCSharp(options, diagramGenerator);
            });
        }

        private int GenerateMermaidErDiagramFromEfCore(MermaidErDiagramFromEfCoreOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid ER diagram", options =>
            {
                var diagramGenerator = new ErDiagramGenerator(options);
                return GenerateMermaidDiagramFromCSharp(options, diagramGenerator);
            });

        }

        private int GenerateMermaidClassDiagramFropmCSharp(MermaidClassDiagramFromCSharpOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid class diagram", options =>
            {
                var diagramGenerator = new ClassDiagramGenerator(new TypeLoaderByReflection(), options);
                return GenerateMermaidDiagramFromCSharp(options, diagramGenerator);
            });
        }

        private int GenerateCSharpFromJsonSchema(CSharpFromJsonSchemaOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "C# code", options =>
            {
                var generator = new CSharpFromJsonSchemaGenerator();
                return generator.Generate(options).Result;
            });
        }

        private int GenerateMermaidClassDiagramFromJsonSchema(MermaidClassDiagramFromJsonSchemaOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid class diagram", options =>
            {
                var generator = new MermaidClassDiagramFromJsonSchemaGenerator();
                return generator.Generate(options).Result;
            });
        }

        private int GenerateMermaidErDiagramFromJsonSchema(MermaidErDiagramFromJsonSchemaOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid ER diagram", options =>
            {
                var generator = new MermaidErDiagramFromJsonSchemaGenerator();
                return generator.Generate(options).Result;
            });
        }

        private int GenerateOptionsFromCommandline(OptionsFromCommandlineOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "dry-gen options", options =>
            {
                var generator = new OptionsFromCommandlineGenerator();
                return generator.Generate(options);
            });
        }

        private TOptions GetOptionsFromFileWithCommandlineOptionsAsOverrides<TOptions>(TOptions commandlineOptions, string[] args) where TOptions : BaseOptions
        {
            if (!string.IsNullOrEmpty(commandlineOptions.OptionsFile))
            {
                var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                var yaml = File.ReadAllText(commandlineOptions.OptionsFile);
                var optionsFromFile = deserializer.Deserialize<TOptions>(yaml);
                // Must read the options from the file twice, since the command line parser clear the collection when the option is not specified on the command line
                var parserResult = parser.ParseArguments(() => deserializer.Deserialize<TOptions>(yaml), args);
                CheckForReparseProblem(parserResult);
                var result = ((Parsed<TOptions>)parserResult).Value;
                ReplaceEmptyIEnumerabeStrings(result, optionsFromFile);
                return result;
            }
            return commandlineOptions;
        }

        [ExcludeFromCodeCoverage] // This should in theory never happend, and cannot be tested
        private void CheckForReparseProblem<TOptions>(ParserResult<TOptions> parserResult) where TOptions : BaseOptions
        {
            if (parserResult.Tag == ParserResultType.NotParsed)
            {
                DisplayHelp(parserResult);
                throw new ArgumentException("Parse problem in reparse after options has been loaded from file");
            }
        }

        private static void ReplaceEmptyIEnumerabeStrings<TOptions>(TOptions target, TOptions source) where TOptions : BaseOptions
        {
            foreach (var property in typeof(TOptions).GetProperties())
            {
                var propertyType = property.PropertyType;
                if (propertyType == typeof(IEnumerable<string>))
                {
                    var targetCollection = (IEnumerable<string>?)property.GetValue(target);
                    var sourceCollection = (IEnumerable<string>?)property.GetValue(source);
                    if (targetCollection?.Any() != true && sourceCollection?.Any() == true)
                    {
                        property.SetValue(target, sourceCollection);
                    }
                }
            }
        }

        private void WriteGeneratedRepresentationToConsoleOrFile(BaseOptions options, string generatedRepresentation)
        {
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                outWriter.Write(generatedRepresentation);
            }
            else
            {
                var outputFile = Path.GetFullPath(options.OutputFile);
                var outputDirectory = Path.GetDirectoryName(outputFile);
                if (outputDirectory == null)
                {
                    throw new InvalidOperationException($"Can't get directory from output file'{outputFile}'.");
                }
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                File.WriteAllText(outputFile, generatedRepresentation);
            }
        }

        [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "We must use LoadFrom to be able to implement this functionallity")]
        private static string GenerateMermaidDiagramFromCSharp(MermaidFromCSharpBaseOptions cSharpOptions, IDiagramGenerator diagramGenerator)
        {
            var assembly = Assembly.LoadFrom(cSharpOptions.InputFile ?? throw new InvalidOperationException("Input file must be specified as the option -i/--input-file on the command line, or as input-file in the option file."));
            var namespaceFilters = cSharpOptions.IncludeNamespaces?.Select(x => new IncludeNamespaceTypeFilter(x)).ToArray() ?? Array.Empty<IncludeNamespaceTypeFilter>();
            var typeFilters = new List<ITypeFilter> { new AnyChildFiltersTypeFilter(namespaceFilters) };
            if (cSharpOptions.IncludeTypeNames?.Any() == true)
            {
                var typeNameFilters = cSharpOptions.IncludeTypeNames.Select(x => new IncludeTypeNameTypeFilter(x)).ToArray();
                typeFilters.Add(new AnyChildFiltersTypeFilter(typeNameFilters));
            }
            if (cSharpOptions.ExcludeTypeNames?.Any() == true)
            {
                var typeNameFilters = cSharpOptions.ExcludeTypeNames.Select(x => new ExcludeTypeNameTypeFilter(x)).ToArray();
                typeFilters.Add(new AllChildFiltersTypeFilter(typeNameFilters));
            }
            var excludePropertyNamesFilters = cSharpOptions.ExcludePropertyNames?.Select(x => new ExcludePropertyNamePropertyFilter(x)).ToArray() ?? Array.Empty<IPropertyFilter>();
            var nameRewriter = new ReplaceNameRewriter(cSharpOptions.NameReplaceFrom ?? string.Empty, cSharpOptions.NameReplaceTo ?? string.Empty);
            return diagramGenerator.Generate(assembly, typeFilters, excludePropertyNamesFilters, nameRewriter);
        }
    }
}