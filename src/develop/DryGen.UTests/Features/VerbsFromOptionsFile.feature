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

Scenario: Should fail when 'configuration' is missing in a yaml document
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		---
		"""
	When I call the program with this command line arguments
		| Arg                            |
		| verbs-from-options-file        |
	Then I should get exit code '1'
	And I should find the text "'configuration' is mandatory in document #1" in console error

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

Scenario: Should generate output with the verb 'csharp-from-json-schema' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: csharp-from-json-schema
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
		//----------------------
		// <auto-generated>
		//     Generated using the NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0) (http://NJsonSchema.org)
		// </auto-generated>
		//----------------------
		
		
		namespace CSharpFromJsonSchema
		{
		    #pragma warning disable // Disable all warnings
		
		    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.9.0.0 (Newtonsoft.Json v13.0.0.0)")]
		    public partial class ClassFromJsonSchema
		    {
		        [Newtonsoft.Json.JsonProperty("prop1", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		        public string Prop1 { get; set; }
		
		
		    }
		}
		"""

Scenario: Should generate output with the verb 'mermaid-class-diagram-from-csharp' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: mermaid-class-diagram-from-csharp
		  options:
		    input-file: $(input_file_one)
		---
		configuration:
		  verb: mermaid-class-diagram-from-csharp
		  options:
		    input-file: $(input_file_one)
		---
		configuration:
		  verb: mermaid-class-diagram-from-csharp
		  options:
		    input-file: $(input_file_two)
		"""
	And this C# source code compiled to a file that is referenced as the environment variable "input_file_one"
		"""
		namespace Test
		{
			public class Customer {}
		}
		"""
	And this C# source code compiled to a file that is referenced as the environment variable "input_file_two"
		"""
		namespace Test
		{
			public class Customer {}
		}
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			class Customer
		classDiagram
			class Customer
		classDiagram
			class Customer
		
		"""

Scenario: Should generate output with the verb 'mermaid-er-diagram-from-csharp' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: mermaid-er-diagram-from-csharp
		  options:
		    input-file: $(input_file_one)
		---
		configuration:
		  verb: mermaid-er-diagram-from-csharp
		  options:
		    input-file: $(input_file_one)
		---
		configuration:
		  verb: mermaid-er-diagram-from-csharp
		  options:
		    input-file: $(input_file_two)
		"""
	And this C# source code compiled to a file that is referenced as the environment variable "input_file_one"
		"""
		namespace Test
		{
			public class Customer {}
		}
		"""
	And this C# source code compiled to a file that is referenced as the environment variable "input_file_two"
		"""
		namespace Test
		{
			public class Customer {}
		}
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		erDiagram
			Customer
		erDiagram
			Customer
		erDiagram
			Customer
		
		"""

Scenario: Should generate output with the verb 'mermaid-er-diagram-from-efcore' in the options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: mermaid-er-diagram-from-efcore
		  options:
		    input-file: $(input_file_one)
		---
		configuration:
		  verb: mermaid-er-diagram-from-efcore
		  options:
		    input-file: $(input_file_one)
		---
		configuration:
		  verb: mermaid-er-diagram-from-efcore
		  options:
		    input-file: $(input_file_two)
		"""
	And this C# source code compiled to a file that is referenced as the environment variable "input_file_one"
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Customer {}
			public class TestDbContext: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasNoKey();
				}
			}
		}
		"""
	And this C# source code compiled to a file that is referenced as the environment variable "input_file_two"
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Customer {}
			public class TestDbContext: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasNoKey();
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
		erDiagram
			Customer
		erDiagram
			Customer
		erDiagram
			Customer
		
		"""

Scenario: Should fail on duplicate name
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		  name: duplicate one
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: duplicate two
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: duplicate two
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: unique
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: duplicate one
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: duplicate two
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "duplicate name(s): 'duplicate one', 'duplicate two'" in console error

Scenario: Should fail when inherits-options-from references unknown name
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		  name: known
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  inherit-options-from: unknown
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "name 'unknown' refrenced in 'inherits-options-from' in document #2 not found" in console error

Scenario: Should fail when inherits-options-from references it self
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		  name: self
		  inherit-options-from: self
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "document #1 'inherit-options-from' it self" in console error

Scenario: Should fail when there is a ring in the inherits-options-from references
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		configuration:
		  verb: options-from-commandline
		  name: one
		  inherit-options-from: two
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: two
		  inherit-options-from: three
		  options:
		    verb: options-from-commandline
		---
		configuration:
		  verb: options-from-commandline
		  name: three
		  inherit-options-from: one
		  options:
		    verb: options-from-commandline
		"""
	When I call the program with this command line arguments
		| Arg                     |
		| verbs-from-options-file |
	Then I should get exit code '1'
	And I should find the text "ring found in 'inherit-options-from' in document #3: 'three' -> 'one' -> 'two' -> 'three'" in console error
