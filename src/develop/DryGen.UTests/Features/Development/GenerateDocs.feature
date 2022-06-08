﻿Feature: Generate Docs for the verbs

To be able to "live as we preach"
As a dry-gen developer
I should be able to generate documentation for each verb from the help system

Scenario: Should generate menu with all verbs
	When I generate the docs verb menu
	Then console out should contain the text
		"""
		# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		- label: dry-gen verbs
		  items:
		  - name: csharp-from-json-schema
		    link: /verbs/csharp-from-json-schema
		  - name: mermaid-class-diagram-from-csharp
		    link: /verbs/mermaid-class-diagram-from-csharp
		  - name: mermaid-class-diagram-from-json-schema
		    link: /verbs/mermaid-class-diagram-from-json-schema
		  - name: mermaid-er-diagram-from-csharp
		    link: /verbs/mermaid-er-diagram-from-csharp
		  - name: mermaid-er-diagram-from-efcore
		    link: /verbs/mermaid-er-diagram-from-efcore
		  - name: mermaid-er-diagram-from-json-schema
		    link: /verbs/mermaid-er-diagram-from-json-schema
		  - name: options-from-commandline
		    link: /verbs/options-from-commandline
		
		"""

Scenario: Should generate verb markdown for options-from-commandline
	When I generate the docs markdown for the verb "options-from-commandline"
	Then console out should contain the text
		"""
		---
		# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		layout: page
		title: options-from-commandline
		description: Details about the dry-gen verb options-from-commandline
		show_sidebar: false
		menubar: verbs_menu
		hero_height: is-fullwidth
		---
		Generate dry-gen options for a verb from the command line. 
		
		## Options
		The verb 'options-from-commandline' uses the following options.
		
		|Option|Alias|Type|Description|
		|---|---|---|---|
		|--input-file|-i|string|Full path to the input file to generate a new representation for.|
		|--options-file|-f|string|Read options from this file.|
		|--output-file|-o|string|Write the generated representation to this file.|
		|--replace-token-in-output-file||string|Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.|
		|--verb||string|The dryg-gen verb to generate options for.|
		
		{% include notification.html
		message="You can always get information about this verb's options by running the command `dry-gen options-from-commandline --help`."
		%}
		## Options file template
		Here is a template for an options file for 'options-from-commandline'. 
		```
		#
		# dry-gen options for verb 'options-from-commandline'
		#
		#input-file: string
		#output-file: string
		#replace-token-in-output-file: string
		#verb: string
		```
		{% include notification.html
		message="You can generate the same template your self with the command `dry-gen options-from-commandline --verb options-from-commandline`."
		%}
		
		"""

# We could test for the docs generation for more verbs, but that would only make more fragile test, since the docs changes each time the help text for a verb is improved...