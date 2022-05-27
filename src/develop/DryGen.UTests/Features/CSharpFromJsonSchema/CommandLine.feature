﻿Feature: Command line for generating C# code from Json schema

A short summary of the feature

Scenario: Should generate c# code to console from 'csharp-from-json-schema' verb
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
		| Arg                     |
		| csharp-from-json-schema |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		//----------------------
		// <auto-generated>
		//     Generated using the NJsonSchema v10.7.1.0 (Newtonsoft.Json v13.0.0.0) (http://NJsonSchema.org)
		// </auto-generated>
		//----------------------
		
		
		namespace CSharpFromJsonSchema
		{
		    #pragma warning disable // Disable all warnings
		
		    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v13.0.0.0)")]
		    public partial class ClassFromJsonSchema
		    {
		        [Newtonsoft.Json.JsonProperty("prop1", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		        public string Prop1 { get; set; }
		
		
		    }
		}
		"""

Scenario: Should generate c# code to file from 'csharp-from-json-schema' verb
	Given this json schema input file with the extension "json"
		"""
		{
			"$schema": "https://json-schema.org/draft/2020-12/schema",
			"id": "https://drygen.net/test-json-schemas/some.json",
			"type": "object",
			"properties": {
				"prop1": {
					"type": "string"
				}
			},
			"additionalProperties": false
		}
		"""
	And output is spesified as a command line argument
	When I call the program with this command line arguments
		| Arg                     |
		| csharp-from-json-schema |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		//----------------------
		// <auto-generated>
		//     Generated using the NJsonSchema v10.7.1.0 (Newtonsoft.Json v13.0.0.0) (http://NJsonSchema.org)
		// </auto-generated>
		//----------------------
		
		
		namespace CSharpFromJsonSchema
		{
		    #pragma warning disable // Disable all warnings
		
		    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v13.0.0.0)")]
		    public partial class ClassFromJsonSchema
		    {
		        [Newtonsoft.Json.JsonProperty("prop1", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		        public string Prop1 { get; set; }
		
		
		    }
		}
		"""

Scenario: Should use option 'schema-file-format'
	Given this json schema input file with the extension "json"
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
		| Arg                     |
		| csharp-from-json-schema |
		| --schema-file-format    |
		| <Schema file format>    |
	Then I should get exit code '<Exit code>'
Examples:
	| Schema file format | Exit code |
	| byextension        | 1         |
	| json               | 1         |
	| yaml               | 0         |

Scenario: Should use option 'namespace'
	Given this json schema input file with the extension "yml"
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
		| Arg                     |
		| csharp-from-json-schema |
		| --namespace             |
		| <Namespace>             |
	Then I should get exit code '0'
	And I should find the text "namespace <Namespace>" in console out
Examples:
	| Namespace          |
	| Test.Namespace.One |
	| Test.Namespace.Two |

Scenario: Should use CSharpFromJsonSchema as namespace when option 'namespace' is not provided
	Given this json schema input file with the extension "yml"
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
		| Arg                     |
		| csharp-from-json-schema |
	Then I should get exit code '0'
	And I should find the text "namespace CSharpFromJsonSchema" in console out

Scenario: Should use option 'root-classname'
	Given this json schema input file with the extension "yml"
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
		| Arg                     |
		| csharp-from-json-schema |
		| --root-classname        |
		| <Root classname>        |
	Then I should get exit code '0'
	And I should find the text "public partial class <Root classname>" in console out
Examples:
	| Root classname          |
	| Test_Root_Classname_One |
	| Test_Root_Classname_Two |

Scenario: Should use schema title as root classname when option r'oot-classname' is not provided
	Given this json schema input file with the extension "yml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.net/test-json-schemas/some.json
		title: Test Schema
		type: object
		properties:
		  prop1:
		    type: string
		additionalProperties: false
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| csharp-from-json-schema |
	Then I should get exit code '0'
	And I should find the text "public partial class TestSchema" in console out

Scenario: Should use ClassFromJsonSchema as root classname when run without option root-classname and schema title is missing
	Given this json schema input file with the extension "yml"
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
		| Arg                     |
		| csharp-from-json-schema |
	Then I should get exit code '0'
	And I should find the text "public partial class ClassFromJsonSchema" in console out

Scenario: Should use options 'array-type' and 'array-instance-type'
	Given this json schema input file with the extension "yml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.net/test-json-schemas/some.json
		type: object
		properties:
		  prop1:
		    type: array
		    items:
		      type: string
		required:
		  - prop1
		additionalProperties: false
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| csharp-from-json-schema |
		| --array-type            |
		| <Array type>            |
		| --array-instance-type   |
		| <Array instance type>   |
	Then I should get exit code '0'
	And I should find the text "public <Array type><string> Prop1 { get; set; } = new <Array instance type><string>();" in console out
Examples:
	| Array type                             | Array instance type                       |
	| System.Collections.Generic.IList       | System.Collections.Generic.List           |
	| System.Collections.Generic.ICollection | System.Collections.ObjectModel.Collection |

Scenario: Should use ICollection and Collection when options 'array-type' and 'array-instance-type' are not provided
	Given this json schema input file with the extension "yml"
		"""
		$schema: https://json-schema.org/draft/2020-12/schema
		id: https://drygen.net/test-json-schemas/some.json
		type: object
		properties:
		  prop1:
		    type: array
		    items:
		      type: string
		required:
		  - prop1
		additionalProperties: false
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| csharp-from-json-schema |
	Then I should get exit code '0'
	And I should find the text "public System.Collections.Generic.ICollection<string> Prop1 { get; set; } = new System.Collections.ObjectModel.Collection<string>();" in console out
