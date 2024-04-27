---
# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
layout: page
title: mermaid-class-diagram-from-csharp
description: Details about the dry-gen verb mermaid-class-diagram-from-csharp
show_sidebar: false
menubar: verbs_menu
hero_height: is-fullwidth
---
Generate a Mermaid Class diagram from a C# assembly using reflection. 

## Options
The verb 'mermaid-class-diagram-from-csharp' uses the following options.

|Option|Alias|Type|Description|
|---|---|---|---|
|--attribute-level||all / public / internal / protected / none|What visibility must an attribute have to be included in the diagram?|
|--direction||default / bt / tb / lr / rl|In what direction should the diagram be generated?|
|--exclude-method-params||true/false|Should method params be excluded from the diagram? (Replaced by count)|
|--exclude-propertynames||List of string|A '; separated' list of regular expressions for property names to exclude from each type.|
|--exclude-static-attributes||true/false|Should static attributes be excluded from the diagram?|
|--exclude-static-methods||true/false|Should static methods be excluded from the diagram?|
|--exclude-typenames||List of string|A '; separated' list of regular expressions for type names to exclude.|
|--include-namespaces||List of string|A list of regular expressions for namespaces to include.|
|--include-typenames||List of string|A '; separated' list of regular expressions for type names to include.|
|--input-file|-i|string|Full path to the input file to generate a new representation for.|
|--method-level||all / public / internal / protected / none|What visibility must a method have to be included in the diagram?|
|--name-replace-from||string|A string to replace in all class/entity names.|
|--name-replace-to||string|The string to replace with in all class/entity names.|
|--options-file|-f|string|Read options from this file.|
|--output-file|-o|string|Write the generated representation to this file.|
|--replace-token-in-output-file||string|Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.|
|--title||string|Diagram title.|
|--tree-shaking-roots||List of string|A '; separated' list of regular expressions for types to keep as roots when tree shaking the resulting diagram.|

{% include notification.html status="is-dark" 
message="You can always get information about this verb's options by running the command 

`dry-gen mermaid-class-diagram-from-csharp --help`."
%}
## Options file template
Here is a template for an options file for 'mermaid-class-diagram-from-csharp'. 
```
#
# dry-gen options for verb 'mermaid-class-diagram-from-csharp'
#
#attribute-level: all | public | internal | protected | none
#direction: default | bt | tb | lr | rl
#exclude-method-params: true|false
#exclude-propertynames: # List of string
#- 
#exclude-static-attributes: true|false
#exclude-static-methods: true|false
#exclude-typenames: # List of string
#- 
#include-namespaces: # List of string
#- 
#include-typenames: # List of string
#- 
#input-file: string
#method-level: all | public | internal | protected | none
#name-replace-from: string
#name-replace-to: string
#output-file: string
#replace-token-in-output-file: string
#title: string
#tree-shaking-roots: # List of string
#- 
```
{% include notification.html status="is-dark" 
message="You can generate the same template your self with the command 

`dry-gen options-from-commandline --verb mermaid-class-diagram-from-csharp`"
%}