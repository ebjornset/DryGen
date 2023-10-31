Feature: Generating Mermaid Class diagram from Json schema

To be able to generate other representation of Json schemas
As a dry-gen user
I should be able to generate Mermaid Class diagram from a Json schema file with the verb 'mermaid-class-diagram-from-json-schema'

# We use csharp-from-json-schema to generate c# under the hoods.
# The options for 'schema-file-format' and 'root-classname' are handled by that generator, 
# so the tests for this functionallity is not repeated here.

Scenario: Should generate Mermaid class diagram code to console from 'mermaid-class-diagram-from-json-schema' verb
	Given this json schema input file with the extension "yaml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.dev/test-json-schemas/some.json
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
		id: https://drygen.dev/test-json-schemas/some.json
		type: object
		properties:
		  prop1:
		    type: string
		additionalProperties: false
		"""
	And output is spesified as a command line argument
	When I call the program with this command line arguments
		| Arg                                    |
		| mermaid-class-diagram-from-json-schema |
		| --direction                            |
		| <Direction>                            |
	Then I should get exit code '0'
	And I should get this generated representation file
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

Scenario: Should generate Mermaid class diagram with tree shaking from 'tree-shaking-roots' option
	Given this json schema input file with the extension "yaml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.dev/test-json-schemas/some.json
		title: Test Schema
		type: object
		properties:
		  Order:
		    $ref: "#/definitions/Order"
		additionalProperties: false
		definitions:
		  Order:
		    additionalProperties: false
		  NoMatchOrder:
		    additionalProperties: false
		"""
	And output is spesified as a command line argument
	When I call the program with this command line arguments
		| Arg                                    |
		| mermaid-class-diagram-from-json-schema |
		| --tree-shaking-roots                   |
		| ^Order$                                |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Order
			class TestSchema
			TestSchema --> "0..1" Order
		
		"""
