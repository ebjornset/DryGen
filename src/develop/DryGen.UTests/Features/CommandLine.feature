Feature: Command line

To be able to use dry-gen with redirection of output and to get help when there is any problems
As a dry-gen user
I should get the generated representation and other error and information message in the console

Scenario: Print usage to console error when command line parsing fails
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --not-a-valid-option              |
	Then I should get exit code '1'
	And I should find the text "Option 'not-a-valid-option' is unknown" in console error

Scenario: Print result to console output when output is not specified
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			class A {
			}
		
		"""

Scenario: Print header to console output when output file is specified
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
			}
		}
		"""
	And output is spesified as a command line argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
	Then I should get exit code '0'
	And I should find the text "Generating Mermaid class diagram to file '" in console out

Scenario: Print usage to console error when an underlying exception is thrown
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
	Then I should get exit code '1'
	And I should find the text "class-diagram-from-csharp" in console error
	And I should find the text "ERROR:" in console error
	And I should find the text "Input file must be specified as the option -i/--input-file on the command line, or as input-file in the option file." in console error
	And I should find the text "Rerun the command with --help to get more help information" in console error

Scenario: Print usage to console error when unknown verb is specified
	When I call the program with this command line arguments
		| Arg              |
		| not-a-valid-verb |
	Then I should get exit code '1'
	And I should find the text "Verb 'not-a-valid-verb' is not recognized." in console error
	And I should find the text "class-diagram-from-csharp" in console error
	And I should find the text "er-diagram-from-csharp" in console error
