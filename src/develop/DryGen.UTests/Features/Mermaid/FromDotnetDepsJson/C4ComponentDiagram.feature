Feature: Generating Mermaid C4 Component diagram from .Net deps.json files

To get a visual good visual overview of the dependencies for a .Net assembly
As a dry-gen user
I should be able to generate Mermaid C4 Component diagram from .Net deps.json file with the verb 'mermaid-c4container-diagram-from-dotnet-deps-json'

Scenario: Should generate Mermaid C4 Component diagram from .Net deps.json to console from 'mermaid-c4container-diagram-from-dotnet-deps-json' verb
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
				},
				".NETCoreApp,Version=v7.0/win-x64": {
					"TestAssembly/1.0.0": {
						"runtime": {
							"TestAssembly.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
			Component("TestAssembly/1.0.0", "TestAssembly/1.0.0")
		
		"""

Scenario: Mermaid C4 Component diagram generation should fail on missing targets
	Given this .Net depts json input file
		"""
		{
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: 'targets' is missing." in console error

Scenario: Mermaid C4 Component diagram generation should fail when targets is an array
	Given this .Net depts json input file
		"""
		{
			"targets": []
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: 'targets' is of unexpected type 'Array', expected 'Object'." in console error

Scenario: Mermaid C4 Component diagram generation should fail on empty targets
	Given this .Net depts json input file
		"""
		{
			"targets": {}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: 'targets' is empty." in console error

Scenario: Mermaid C4 Component diagram generation should fail when targets only have empty properties
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
				},
				".NETCoreApp,Version=v7.0/win-x64": {
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: Found no non empty 'target'." in console error

Scenario: Mermaid C4 Component diagram generation should fail when target only have assemblies without 'runtime' property
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"TestAsembly/1.0.0": {
					}
				},
				".NETCoreApp,Version=v7.0/win-x64": {
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: Found no non empty runtime dependencies in target '.NETCoreApp,Version=v7.0'." in console error

Scenario: Mermaid C4 Component diagram generation should fail when target only have assemblies with empty 'runtime' property
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"TestAsembly/1.0.0": {
						"dependencies": {
						},
						"runtime": {
						}
					}
				},
				".NETCoreApp,Version=v7.0/win-x64": {
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: Found no non empty runtime dependencies in target '.NETCoreApp,Version=v7.0'." in console error
