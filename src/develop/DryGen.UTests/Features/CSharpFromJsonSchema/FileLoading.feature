Feature: Loading Json schema files

To be able to generate other representation of Json schemas from both Json and Yaml format
As a dry-gen user
I should be able to load the schema from both Json and Yaml files 

Scenario: Loading json schema file from yaml file by extension
	Given this json schema
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.dev/test-json-schemas/some.json
		title: Test Schema
		type: object
		properties:
		  prop1:
		    type: string
		"""
	When I load the json schema from a file with the extension "<Extension>"
	Then I should not get an exception
Examples:
	| Extension |
	| yml       |
	| yaml      |

Scenario: Loading json schema file from json file by extension
	Given this json schema
		"""
		{
			"$schema": "https://json-schema.org/draft/2020-12/schema",
			"id": "https://drygen.dev/test-json-schemas/some.json",
			"title": "Test Schema",
			"type": "object",
			"properties": {
				"prop1": {
					"type": "string"
				}
			}
		}
		"""
	When I load the json schema from a file with the extension "<Extension>"
	Then I should not get an exception
Examples:
	| Extension |
	| json      |
	| not-yaml  |

Scenario: Force loading of json schema as yaml
	Given this json schema
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.dev/test-json-schemas/some.json
		title: Test Schema
		type: object
		properties:
		  prop1:
		    type: string
		  $schema:
		    description: Optional reference to this json schema. (It's only included in this schema to make 'additionalProperties' work for the root object.)
		    type: string
		"""
	When I load the json schema from a file forcing the schema format "yaml"
	Then I should not get an exception

Scenario: Force loading of json schema as json
	Given this json schema
		"""
		{
			"$schema": "https://json-schema.org/draft/2020-12/schema",
			"id": "https://drygen.dev/test-json-schemas/some.json",
			"title": "Test Schema",
			"type": "object",
			"properties": {
				"prop1": {
					"type": "string"
				}
			}
		}
		"""
	When I load the json schema from a file forcing the schema format "json"
	Then I should not get an exception
