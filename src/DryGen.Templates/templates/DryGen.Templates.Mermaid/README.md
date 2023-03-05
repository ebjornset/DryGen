# Project template for generating Mermaid diagrams from C# code with dry-gen

This project contains examples of how you can integrate generation of Mermaid diagrams from C# code with dry-gen into your development process using [MSBuild Targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-targets) in the .csproj file.

The different parts of this example are

## [The generated Mermaid diagrams](docs/README.md)

## [The .csproj file](./DryGen.Templates.Mermaid.csproj)

## [The example C# code used](./Example.cs)

## The options files

The different `dotnet dry-gen` exec tasks in the .csproj file uses options files to customize most of the generation.

(NB! The input and output files are kept as parameters in the .csproj file, to make it easy to customize them with MSBuild properties.)

The option files were generated with the dry-gen verb [options-from-commandline](https://docs.drygen.dev/verbs/options-from-commandline/), and then customized to produce the wanted diagrams.

You can customize these further to adjust the diagrams so they suites your needs. Just save the options file, build the project (e.g. with `dotnet build`) and reload the output file in your editor/viewer to se the result.

### [Options file for Mermaid class diagram from C#](./options/mermaid-class-diagram-from-csharp.yaml).

### [Options file for Mermaid ER diagram from EF Core](./options/mermaid-er-diagram-from-efcore.yaml).

### [Options file for Mermaid class diagram from the DbContext](./options/mermaid-class-diagram-from-dbcontext.yaml).

To read more about dry-gen, head over to the [documentation](https://docs.drygen.dev/).

Have fun!
