---
# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
layout: page
title: mermaid-er-diagram-from-efcore
description: Details about the dry-gen verb mermaid-er-diagram-from-efcore
show_sidebar: false
menubar: verbs_menu
hero_height: is-fullwidth
---
Generate a Mermaid Entity Relationship diagram from a C# assembly using Entity Framework Core. 

## Options
The verb 'mermaid-er-diagram-from-efcore' uses the following options.

|Option|Alias|Type|Description|
|---|---|---|---|
|--attribute-type-exclusion||none / foreignkeys / all|What kind of attributes should be excluded from the diagram?|
|--exclude-attribute-comments||true/false|Should attributes comments be excluded from the diagram?|
|--exclude-attribute-keytypes||true/false|Should attributes key types be excluded from the diagram?|
|--exclude-propertynames||List of string|A '; separated' list of regular expressions for property names to exclude from each type.|
|--exclude-typenames||List of string|A '; separated' list of regular expressions for type names to exclude.|
|--include-namespaces||List of string|A list of regular expressions for namespaces to include.|
|--include-typenames||List of string|A '; separated' list of regular expressions for type names to include.|
|--input-file|-i|string|Full path to the input file to generate a new representation for.|
|--name-replace-from||string|A string to replace in all class/entity names.|
|--name-replace-to||string|The string to replace with in all class/entity names.|
|--options-file|-f|string|Read options from this file.|
|--output-file|-o|string|Write the generated representation to this file.|
|--output-template||string|Template text for controlling the final output. Use ${DryGenOutput} to include the generated representation in the result|
|--relationship-type-exclusion||none / all|What kind of relationships should be excluded from the diagram?|
|--replace-token-in-output-file||string|Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.|
|--title||string|Diagram title.|
|--tree-shaking-roots||List of string|A '; separated' list of regular expressions for types to keep as roots when tree shaking the resulting diagram.|

{% include notification.html status="is-dark" 
message="You can always get information about this verb's options by running the command 

`dry-gen mermaid-er-diagram-from-efcore --help`."
%}
## Options file template
Here is a template for an options file for 'mermaid-er-diagram-from-efcore'. 
```
#
# dry-gen options for verb 'mermaid-er-diagram-from-efcore'
#
#attribute-type-exclusion: none | foreignkeys | all
#exclude-attribute-comments: true|false
#exclude-attribute-keytypes: true|false
#exclude-propertynames: # List of string
#- 
#exclude-typenames: # List of string
#- 
#include-namespaces: # List of string
#- 
#include-typenames: # List of string
#- 
#input-file: string
#name-replace-from: string
#name-replace-to: string
#output-file: string
#output-template: string
#relationship-type-exclusion: none | all
#replace-token-in-output-file: string
#title: string
#tree-shaking-roots: # List of string
#- 
```
{% include notification.html status="is-dark" 
message="You can generate the same template your self with the command 

`dry-gen options-from-commandline --verb mermaid-er-diagram-from-efcore`"
%}