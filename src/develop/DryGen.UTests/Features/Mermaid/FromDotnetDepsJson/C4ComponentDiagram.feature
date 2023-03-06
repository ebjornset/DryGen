Feature: Generating Mermaid C4 Component diagram from .Net deps.json files

To get a visual good visual overview of the dependencies for a .Net assembly
As a dry-gen user
I should be able to generate Mermaid C4 Component diagram from .Net deps.json file with the verb 'mermaid-c4container-diagram-from-dotnet-deps-json'

Scenario: Should generate Mermaid C4 Component diagram from .Net deps.json to console from 'mermaid-c4container-diagram-from-dotnet-deps-json' verb
	Given this .Net depts json input file
		"""
		{
			"targets": {}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		
		"""
