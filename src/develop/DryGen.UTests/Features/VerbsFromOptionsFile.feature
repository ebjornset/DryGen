Feature: Verbs from options file

To avoid many subsequent runs of dry-gen, and to speed up my development cycle
As a dry-gen user
I should be able to execute several verbs in one run based on several yaml documents in the options file

Scenario: Should generate output from one yaml document in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
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

Scenario: Should generate output from two yaml documents in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '0'
	And console out should contain the text
	# NB! output is written to the console without a ending newline, 
	# so the second document is expected to start at the same line as the first one ends.
		"""
		#
		# dry-gen options for verb 'options-from-commandline'
		#
		#output-file: string
		#replace-token-in-output-file: string
		#verb: string#
		# dry-gen options for verb 'options-from-commandline'
		#
		#output-file: string
		#replace-token-in-output-file: string
		#verb: string
		"""

Scenario: Should fail when '--options--file' argument is missing
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "--options-file is mandatory" in console error

Scenario: Should fail when 'options' is missing in a yaml document
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "'configuration.options' is mandatory in document #1" in console error

Scenario: Should fail for unknown 'verb' in a yaml document without 'options'
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: unknown
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "Unknown 'verb' in document #1" in console error

Scenario: Should fail for unknown 'verb' in a yaml document with 'options'
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: unknown
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "Unknown 'verb' in document #1" in console error

Scenario: Should generate output with the verb 'mermaid-c4component-diagram-from-dotnet-deps-json' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: mermaid-c4component-diagram-from-dotnet-deps-json
		  options:
		    input-file: $(input_file)
		"""
	And this file is referenced as the environment variable "input_file"
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")

		"""

Scenario: Should generate output with the verb 'mermaid-class-diagram-from-json-schema' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: mermaid-class-diagram-from-json-schema
		  options:
		    input-file: $(input_file)
		    schema-file-format: yaml
		"""
	And this file is referenced as the environment variable "input_file"
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
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			class ClassFromJsonSchema {
				+string Prop1
			}
		
		"""

Scenario: Should generate output with the verb 'mermaid-er-diagram-from-json-schema' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: mermaid-er-diagram-from-json-schema
		  options:
		    input-file: $(input_file)
		    schema-file-format: yaml
		"""
	And this file is referenced as the environment variable "input_file"
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
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
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