﻿Feature: Generate Docs for the verbs

To be able to "live as we preach"
As a dry-gen developer
I should be able to generate documentation for each verb from the help system

Scenario: Should generate menu with all verbs
	When I generate the docs verb menu
	Then console out should contain the text
		"""
		# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		- name: Overview
		  href: index.md
		- name: csharp-from-json-schema
		  href: csharp-from-json-schema.md
		- name: mermaid-c4component-diagram-from-dotnet-deps-json
		  href: mermaid-c4component-diagram-from-dotnet-deps-json.md
		- name: mermaid-class-diagram-from-csharp
		  href: mermaid-class-diagram-from-csharp.md
		- name: mermaid-class-diagram-from-json-schema
		  href: mermaid-class-diagram-from-json-schema.md
		- name: mermaid-er-diagram-from-csharp
		  href: mermaid-er-diagram-from-csharp.md
		- name: mermaid-er-diagram-from-efcore
		  href: mermaid-er-diagram-from-efcore.md
		- name: mermaid-er-diagram-from-json-schema
		  href: mermaid-er-diagram-from-json-schema.md
		- name: options-from-commandline
		  href: options-from-commandline.md
		- name: verbs-from-options-file
		  href: verbs-from-options-file.md
		
		"""

Scenario: Should generate verb markdown for options-from-commandline
	When I generate the docs markdown for the verb "options-from-commandline"
	Then console out should contain the text
		"""
		<!--
		Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		-->
		# options-from-commandline
		Generate dry-gen options for a verb from the command line. 
		
		## Options
		The verb uses the following options.
		
		|Option|Alias|Type|Description|
		|---|---|---|---|
		|--options-file|-f|string|Read options from this file.|
		|--output-file|-o|string|Write the generated representation to this file.|
		|--output-template||string|Template text for controlling the final output. Use ${DryGenOutput} to include the generated representation in the result|
		|--replace-token-in-output-file||string|Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.|
		|--verb||string|The dryg-gen verb to generate options for.|
		
		>[!TIP]
		>You can always get information about this verb's options by running the command
		>
		>`dry-gen options-from-commandline --help`
		## Options file template
		Here is a template for an options file for 'options-from-commandline'. 
		```
		#
		# dry-gen options for verb 'options-from-commandline'
		#
		#output-file: string
		#output-template: string
		#replace-token-in-output-file: string
		#verb: string
		```
		>[!TIP]
		>You can generate the same template your self with the command
		>
		>`dry-gen options-from-commandline --verb options-from-commandline`
		
		"""

# We could test for the docs generation for more verbs, but that would only make more fragile test, since the docs changes each time the help text for a verb is improved...