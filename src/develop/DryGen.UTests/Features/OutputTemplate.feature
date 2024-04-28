Feature: Output template

To be able to use the generated representation in different scenarios
As a dry-gen user
I should be able to specify an output template with extra text around the raw DryGen output

Background:
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
			}
		}
		"""

Scenario: Raw output is the default
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		classDiagram
			class A
		
		"""

Scenario: Should use output template from command line argument
	When I call the program with this command line arguments
		| Arg                                                                                 |
		| mermaid-class-diagram-from-csharp                                                   |
		| --output-template                                                                   |
		| start of template\n${DryGenOutput}\nsecond output\n${DryGenOutput}\nend of template |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		start of template
		classDiagram
			class A
		
		second output
		classDiagram
			class A
		
		end of template
		"""

Scenario: Should use output template from options file
	Given this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		input-file: Should be overridden by command line
		output-template: |
		  start of template
		  ${DryGenOutput}
		  second output
		  ${DryGenOutput}
		  end of template
		"""
	When I call the program with this command line arguments
		| Arg                                                                                 |
		| mermaid-class-diagram-from-csharp                                                   |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		start of template
		classDiagram
			class A
		
		second output
		classDiagram
			class A
		
		end of template
		"""
