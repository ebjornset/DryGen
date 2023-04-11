@notParallel
Feature: Error handeling when generating Mermaid C4 Component diagram from .Net deps.json files

To understand why generation of Mermaid C4 Component diagram from .Net deps.json files fails
As a dry-gen user
I would like to get godd error messages when the .Net dps.json file has unexpected content


Scenario: Mermaid C4 Component diagram generation should fail on missing 'targets'
	Given this .Net depts json input file
		"""
		{
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: 'targets' is missing." in console error

Scenario: Mermaid C4 Component diagram generation should fail on empty 'targets'
	Given this .Net depts json input file
		"""
		{
			"targets": {}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
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
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: Found no non empty 'target'." in console error

Scenario: Mermaid C4 Component diagram generation should fail when selected target only have assemblies without 'runtime' property
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
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: Found no non empty runtime dependencies in target '.NETCoreApp,Version=v7.0'." in console error

Scenario: Mermaid C4 Component diagram generation should fail when selected target only have assemblies with empty 'runtime' property
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
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '1'
	And I should find the text "Invalid deps.json: Found no non empty runtime dependencies in target '.NETCoreApp,Version=v7.0'." in console error
