Feature: Generating Mermaid ER diagram from Json schema

To be able to generate other representation of Json schemas
As a dry-gen user
I should be able to generate Mermaid ER diagram from a Json schema file with the verb 'mermaid-er-diagram-from-json-schema'

# We use csharp-from-json-schema to generate c# under the hoods.
# The options for 'schema-file-format' and 'root-classname' are handled by that generator, 
# so the tests for this functionallity is not repeated here.

Background:
	Given this json schema input file with the extension "yaml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.dev/test-json-schemas/some.json
		type: object
		properties:
		  prop1:
		    type: string
		  test:
		    $ref: "#/definitions/test"
		additionalProperties: false
		definitions:
		  test:
		    properties:
		      prop2:
		        type: integer
		    required:
		      - prop2
		"""

Scenario: Should generate Mermaid ER diagram code to console from 'mermaid-er-diagram-from-json-schema' verb
	When I call the program with this command line arguments
		| Arg                                 |
		| mermaid-er-diagram-from-json-schema |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		erDiagram
			ClassFromJsonSchema {
				string Prop1
			}
			Test {
				int Prop2
			}
			Test ||..|| ClassFromJsonSchema : ""
		
		"""

Scenario: Should generate Er diagram from json schema with attributes excluded from argument
	Given output is spesified as a command line argument
	When I call the program with this command line arguments
		| Arg                                 |
		| mermaid-er-diagram-from-json-schema |
		| --exclude-all-attributes            |
		| true                                |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			ClassFromJsonSchema
			Test
			Test ||..|| ClassFromJsonSchema : ""
		
		"""

Scenario: Should generate Er diagram from json schema with relationships excluded from argument
	When I call the program with this command line arguments
		| Arg                                 |
		| mermaid-er-diagram-from-json-schema |
		| --exclude-all-relationships         |
		| true                                |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		erDiagram
			ClassFromJsonSchema {
				string Prop1
			}
			Test {
				int Prop2
			}
		
		"""

Scenario: Should generate Mermaid Er diagram with tree shaking from 'tree-shaking-roots' option
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
		| mermaid-er-diagram-from-json-schema |
		| --tree-shaking-roots                   |
		| ^Order$                                |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			Order
			TestSchema
			Order ||..|| TestSchema : ""
		
		"""
