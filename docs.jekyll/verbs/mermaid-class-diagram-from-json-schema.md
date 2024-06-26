---
# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
layout: page
title: mermaid-class-diagram-from-json-schema
description: Details about the dry-gen verb mermaid-class-diagram-from-json-schema
show_sidebar: false
menubar: verbs_menu
hero_height: is-fullwidth
---
Generate a Mermaid Class diagram from a json schema. 

## Options
The verb 'mermaid-class-diagram-from-json-schema' uses the following options.

|Option|Alias|Type|Description|
|---|---|---|---|
|--direction||default / bt / tb / lr / rl|In what direction should the diagram be generated?|
|--input-file|-i|string|Full path to the input file to generate a new representation for.|
|--options-file|-f|string|Read options from this file.|
|--output-file|-o|string|Write the generated representation to this file.|
|--output-template||string|Template text for controlling the final output. Use ${DryGenOutput} to include the generated representation in the result|
|--replace-token-in-output-file||string|Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.|
|--root-classname||string|The classname for the class representing the schema it self. Default is the schema title, or 'ClassFromJsonSchema' if the schema has no title.|
|--schema-file-format||byextension / json / yaml|What format should be used when reading the schema file? 'ByExtension' (default) treats files with extension 'yaml' or 'yml' as yaml, others as json. Use 'Yaml' or 'Json' to force the format explicitly.|
|--title||string|Diagram title.|
|--tree-shaking-roots||List of string|A '; separated' list of regular expressions for types to keep as roots when tree shaking the resulting diagram.|

{% include notification.html status="is-dark" 
message="You can always get information about this verb's options by running the command 

`dry-gen mermaid-class-diagram-from-json-schema --help`."
%}
## Options file template
Here is a template for an options file for 'mermaid-class-diagram-from-json-schema'. 
```
#
# dry-gen options for verb 'mermaid-class-diagram-from-json-schema'
#
#direction: default | bt | tb | lr | rl
#input-file: string
#output-file: string
#output-template: string
#replace-token-in-output-file: string
#root-classname: string
#schema-file-format: byextension | json | yaml
#title: string
#tree-shaking-roots: # List of string
#- 
```
{% include notification.html status="is-dark" 
message="You can generate the same template your self with the command 

`dry-gen options-from-commandline --verb mermaid-class-diagram-from-json-schema`"
%}
