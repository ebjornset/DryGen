Feature: ToolInvocation

A short summary of the feature

Scenario: Should be able to run the dotnet tool
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
			}
		}
		"""
	And this input file as a command line option
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		include-namespaces:
		- .*
		include-typenames:
		- .*
		"""
	When I call the tool with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
	Then I should get no errors from the tool
	And I should get this output from the tool
		"""
		classDiagram
			class A {
			}
		
		"""
	And I should get exit code '0' from the tool
