---
layout: page
title: Getting started
description: How to get started with dry-gen
toc: true
hero_height: is-fullwidth
---

DryGen is a .Net tool to generate other representations of a piece of knowledge from one representation.

It's inspired by the ["Don't repeat yourself" (DRY) prinsiple](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself){:target="\_blank"} "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system"

Beeing a .Net tool (with tool name `dry-gen`), you'll need a [.NET SDK](https://dotnet.microsoft.com/en-us/download){:target="\_blank"} installed on your machine to use it. With a .Net SDK in place `dry-gen` is installed and used like any other [.Net tool](https://aka.ms/global-tools){:target="\_blank"}.

**A little warning befor you start**: As with all .Net tools, you run `dry-gen` as an application with full trust, so make sure you know exactly where you write your output files.

## Installation

#### As a global tool

```
dotnet tool install --global dry-gen
```

#### As a local tool

```
dotnet new tool-manifest # if you haven't set up the .Net tools for this project already.
dotnet tool install --local dry-gen
```

## Use

You invoke DryGen from the commandline as `dry-gen` when its installed as a global tool, and with `dotnet dry-gen` when its installed as a local tool.

The general structure of the command line is `dry-gen <verb> [options]`

##### Verb

The verb defines your target and source representation. The supported verbs are using the pattern `<target-representation>-from-<source-representation>`, e.g. `mermaid-class-diagram-from-csharp`. Execute `dry-gen` without any parameters to get a list of the supported verbs.

##### Options

The options are spesific to each verb, and are used to fine tune the result representation. Most options uses the standard long notation of `--<option-name>`, e.g. `--input-file`. Some options also supports the shorthand notation of `-<letter>`, e.g. `-i` for input file, but we suggest you use the long format. Execute `dry-gen <verb> --help` to get the list of options for a specific verb, e.g. `dry-gen mermaid-class-diagram-from-csharp --help`

##### Explore the dry-gen features

Head over to the [verbs page](/verbs) to see the list of verbs dry-gen supports, or take a look at [our examples](/examples).

## Integrate dry-gen into your development process

dry-gen contains some example .Net templates that you can use with the [`dotnet new` command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new){:target="\_blank"}

```
dotnet new install dry-gen.templates
```

Have fun!
