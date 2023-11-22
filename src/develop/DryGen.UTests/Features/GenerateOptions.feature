Feature: Generate options from command line

To be able to use options files without having to hand craft them from scratch
As a dry-gen user
I should be able to generate option files for all verbs with the verb 'options-from-commandline'

Scenario: Verb should be required
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
	Then I should get exit code '1'

Scenario: Verb must be a known dry-gen verb
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
		| --verb                   |
		| not-a-valid-dry-gen-verb |
	Then I should get exit code '1'

Scenario: Should generate options for verb csharp-from-json-schema
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
		| --verb                   |
		| csharp-from-json-schema  |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'csharp-from-json-schema'
		#
		#array-instance-type: string
		#array-type: string
		#input-file: string
		#namespace: string
		#output-file: string
		#replace-token-in-output-file: string
		#root-classname: string
		#schema-file-format: byextension | json | yaml
		"""

Scenario: Should generate options for verb mermaid-class-diagram-from-csharp
	When I call the program with this command line arguments
		| Arg                               |
		| options-from-commandline          |
		| --verb                            |
		| mermaid-class-diagram-from-csharp |
	Then I should get exit code '0'
	And console out should contain the text
		"""
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
		#tree-shaking-roots: # List of string
		#- 
		"""
		
Scenario: Should generate options for verb mermaid-class-diagram-from-json-schema
	When I call the program with this command line arguments
		| Arg                                    |
		| options-from-commandline               |
		| --verb                                 |
		| mermaid-class-diagram-from-json-schema |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'mermaid-class-diagram-from-json-schema'
		#
		#direction: default | bt | tb | lr | rl
		#input-file: string
		#output-file: string
		#replace-token-in-output-file: string
		#root-classname: string
		#schema-file-format: byextension | json | yaml
		#tree-shaking-roots: # List of string
		#- 
		"""

Scenario: Should generate options for verb mermaid-er-diagram-from-csharp
	When I call the program with this command line arguments
		| Arg                            |
		| options-from-commandline       |
		| --verb                         |
		| mermaid-er-diagram-from-csharp |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'mermaid-er-diagram-from-csharp'
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
		#relationship-type-exclusion: none | all
		#replace-token-in-output-file: string
		#tree-shaking-roots: # List of string
		#- 
		"""

Scenario: Should generate options for verb mermaid-er-diagram-from-efcore
	When I call the program with this command line arguments
		| Arg                            |
		| options-from-commandline       |
		| --verb                         |
		| mermaid-er-diagram-from-efcore |
	Then I should get exit code '0'
	And console out should contain the text
		"""
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
		#relationship-type-exclusion: none | all
		#replace-token-in-output-file: string
		#tree-shaking-roots: # List of string
		#- 
		"""

Scenario: Should generate options for verb mermaid-er-diagram-from-json-schema
	When I call the program with this command line arguments
		| Arg                                 |
		| options-from-commandline            |
		| --verb                              |
		| mermaid-er-diagram-from-json-schema |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'mermaid-er-diagram-from-json-schema'
		#
		#exclude-all-attributes: true|false
		#exclude-all-relationships: true|false
		#input-file: string
		#output-file: string
		#replace-token-in-output-file: string
		#root-classname: string
		#schema-file-format: byextension | json | yaml
		#tree-shaking-roots: # List of string
		#- 
		"""
		
Scenario: Should generate options for verb options-from-commandline
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
		| --verb                   |
		| options-from-commandline |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'options-from-commandline'
		#
		#output-file: string
		#replace-token-in-output-file: string
		#verb: string
		"""

Scenario: Should generate options for verb verbs-from-options-file
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
		| --verb                   |
		| verbs-from-options-file  |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'verbs-from-options-file'
		#

		"""
