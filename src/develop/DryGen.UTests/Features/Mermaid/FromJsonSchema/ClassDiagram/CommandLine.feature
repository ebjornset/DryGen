Feature: Command line for generating Mermaid class diagram from Json schema

A short summary of the feature

# We use csharp-from-json-schema to generate c# under the hoods.
# The options for 'schema-file-format' and 'root-classname' are handled by that generator, 
# so the tests for this functionallity is not repeated here.

Scenario: Should generate Mermaid class diagram code to console from 'mermaid-class-diagram-from-json-schema' verb
	Given this json schema input file with the extension "yaml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.net/test-json-schemas/some.json
		type: object
		properties:
		  prop1:
		    type: string
		additionalProperties: false
		"""
	When I call the program with this command line arguments
		| Arg                                    |
		| mermaid-class-diagram-from-json-schema |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			class ClassFromJsonSchema {
				+string Prop1
			}
		
		"""

Scenario: Should generate Mermaid class diagram with direction from 'direction' option
	Given this json schema input file with the extension "yaml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.net/test-json-schemas/some.json
		type: object
		properties:
		  prop1:
		    type: string
		additionalProperties: false
		"""
	When I call the program with this command line arguments
		| Arg                                    |
		| mermaid-class-diagram-from-json-schema |
		| --direction                       |
		| <Direction>                       |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			direction <Direction>
			class ClassFromJsonSchema {
				+string Prop1
			}
		
		"""
Examples:
	| Direction |
	| TB        |
	| BT        |
	| LR        |
	| RL        |
