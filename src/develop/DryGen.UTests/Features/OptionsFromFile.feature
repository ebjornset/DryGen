Feature: Options from file

To avoid long command lines, and to be able to reuse option sets
As a dry-gen user
I should be able to specify the options in a Yaml file

Background:
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
				public int ExcludedProp { get;set;}
			}
			public class ExcludedOne {
			}
			public class ExcludedTwo {
			}
		}
		"""
	And this input file as a command line option
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		input-file: Should be overridden by command line
		include-namespaces:
		- .*
		include-typenames:
		- .*
		exclude-typenames: 
		- ^ExcludedOne$ 
		- ^ExcludedTwo$ 
		name-replace-from: A
		name-replace-to: AReplaced
		exclude-propertynames:
		- ^ExcludedProp$
		"""

Scenario: Read options from file
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --exclude-static-attributes       |
		| true                              |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			class AReplaced {
			}
		
		"""

Scenario: Command line options overrides options from file
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-typenames            |
		| ^NotAMatch$                    |
		| --exclude-propertynames        |
		| ^NotAMatch$                    |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		erDiagram
			AReplaced {
				int ExcludedProp
			}
			ExcludedOne
			ExcludedTwo
		
		"""

Scenario: Command line should not fail if options file is empty
	And this input file as a command line option
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		#
		"""
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-typenames            |
		| ^NotAMatch$                    |
		| --exclude-propertynames        |
		| ^NotAMatch$                    |
	Then I should get exit code '0'

Scenario: All class diagram options can be specified in a file
	Given this input file as a command line option
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		attribute-level: internal
		method-level: public
		direction: tb
		exclude-static-attributes: true
		exclude-static-methods: false
		exclude-method-params: true
		"""
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
	Then I should get exit code '0'

Scenario: All Er diagram options can be specified in a file
	Given this input file as a command line option
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		exclude-attribute-keytypes: false
		exclude-attribute-comments: true
		attribute-type-exclusion: none
		relationship-type-exclusion: none
		"""
	When I call the program with this command line arguments
		| Arg    |
		| <Verb> |
	Then I should get exit code '0'
Examples:
	| Verb                           |
	| mermaid-er-diagram-from-csharp |
	| mermaid-er-diagram-from-efcore |
