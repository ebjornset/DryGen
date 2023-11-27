﻿using DryGen.Options;
using YamlDotNet.Serialization;

namespace DryGen.Features.VerbsFromOptionsFile;

public abstract class VerbsFromOptionsFileConfiguration<TOptions> : IVerbsFromOptionsFileConfiguration where TOptions : CommonOptions
{
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string? Name { get; set; }
    [YamlMember(Alias = "verb", ApplyNamingConventions = false)]
    public string? Verb { get; set; }
    [YamlMember(Alias = "options", ApplyNamingConventions = false)]
    public TOptions? Options { get; set; }

    public CommonOptions? GetOptions() 
    {
        return Options;
    }
}

