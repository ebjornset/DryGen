# Project template for generating C# code and Mermid diagrams from a json schema with dry-gen

This project contains examples of how you can integrate generation of C# code and Mermaid diagrams from a json schema with dry-gen into your development process using [MSBuild Targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-targets) in the .csproj file.

The different parts of this example are

## [The generated C# code](./Example.generated.cs)

## [The generated Mermid diagrams](./docs/README.md)

## [The .csproj file](./DryGen.Templates.JsonSchema.csproj)

## [The example json schema used (as yaml...)](./Example.yaml)

## The options files

The different `dotnet dry-gen` exec tasks in the .csproj file uses options files to customize most of the generation.

(NB! The input and output files are kept as parameters in the .csproj file, to make it easy to customize them with MSBuild properties.)

The option files were generated with the dry-gen verb [options-from-commandline](https://docs.drygen.dev/verbs/options-from-commandline/), and then customized to produce the wanted diagrams.

You can customize these further to adjust the diagrams so they suites your needs. Just save the options file, build the project (e.g. with `dotnet build`) and reload the output file in your editor/viewer to se the result.

### [Options file for C# code from json schema](./options/mermaid-class-diagram-from-json-schema.yaml).

### [Options file for Mermaid class diagram from json schema](./options/mermaid-class-diagram-from-json-schema.yaml).

To read more about dry-gen, head over to the [documentation](https://docs.drygen.dev/).

Have fun!
