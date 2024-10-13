---
_disableToc: true
---
# About
The dry-gen tool is developed as an open source software project hosted on [Github](https://github.com/ebjornset/DryGen) under the [MIT licence](https://github.com/ebjornset/DryGen/blob/main/LICENSE.md).

## Inspiration

Beside the ["don't repeat yourself" (DRY) prinsiple](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself), formulated in the book [The Pragmatic Programmer](https://en.wikipedia.org/wiki/The_Pragmatic_Programmer), [Mermaid (.js)](https://mermaid-js.github.io/mermaid/#/) is the main inspiration for dry-gen. Their documentation formats are so terse and simple that I was able to get my first diagram up and running in a few minutes by just typing plain text in my editor! Knowing a bit about C# reflection I realized that generating Mermaid diagrams from C# assemblies would be a managable task, and thus dry-gen was borned.

## Development tools

As an open source tool **"we're standing on the shoulder of giants"**, and we heavily rely on the hard work of others volunteers. Drygen would probably never have seen the light of day without these other excellent tools:

- The main developement tool is [.Net](https://dotnet.microsoft.com/), a free, cross-platform, and open source developer platform.
- Github is used for our [code repository](https://github.com/ebjornset/DryGen), [issue tracker](https://github.com/ebjornset/DryGen/issues), [deployment tool](https://github.com/ebjornset/DryGen/actions) and [documentation site](https://docs.drygen.dev/)
- We use [SonarCloud](https://sonarcloud.io/project/overview?id=ebjornset_DryGen) (free for open source projects) as our static code analysis tool.
- We use [Mend bolt](https://www.mend.io/free-developer-tools/bolt/) (free for open source projects) and [Snyk](https://snyk.io/) as our security alerts and compliance tools.
- The [documentation site](https://docs.drygen.dev/) is generated at deploy time using the [docfx](https://dotnet.github.io/docfx/) engine.
- As a commandline tool, we rely heavily on the [Command Line Parser Library](https://github.com/commandlineparser/commandline).
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) is used to implement the `--options-file` option.
- The verb `csharp-from-json-schema` is using [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) as the internal engine doing all the hard work.
- We use [Nuke](https://nuke.build/) as our build tool, and to generate our [Github actions](https://github.com/ebjornset/DryGen/actions).
- We use [xunit.net](https://xunit.net/) as our test running framework
- [Fluent Assertions](https://fluentassertions.com/) is used to assert the code is doing what we expects.
- Most of the tests are written as [Behavioral specifications](https://en.wikipedia.org/wiki/Behavior-driven_development#Behavioral_specifications) using [Reqnroll](https://reqnroll.net/).