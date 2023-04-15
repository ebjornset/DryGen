@notParallel
Feature: Controlling the generation of Mermaid C4 Component diagram from .Net deps.json files with assembly filters

To be able to control the content of the generated diagram
As a dry-gen user
I should be able to control how the Mermaid C4 Component diagram is generated from .Net deps.json file with assembly filters

# All scenarios in this feature uses '--relation-level none -–boundary-level none --title "not important"' to simplify the output, so ite easier to focus on the included assemblies 

Background:
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"dependencies": {
							"Dependency.One.SubOne": "1.0.0"
						},
						"runtime": {
							"MainAssembly.dll": {}
						}
					},
					"Dependency.One/1.0.0": {
						"dependencies": {
							"Dependency.One.SubOne": "1.0.0",
							"Dependency.One.SubTwo": "1.0.0"
						},
						"runtime": {
							"Dependency.One.dll": {}
						}
					},
					"Dependency.One.SubOne/1.0.0": {
						"dependencies": {
							"Dependency.One.SubOne.SubSubOne": "1.0.0",
							"Dependency.One.SubOne.SubSubTwo": "1.0.0"
						},
						"runtime": {
							"Dependency.One.SubOne.dll": {}
						}
					},
					"Dependency.One.SubOne.SubSubOne/1.0.0": {
						"dependencies": {
							"Dependency.One.SubOne.SubSubTwo": "1.0.0"
						},
						"runtime": {
							"Dependency.One.SubOne.SubSubOne.dll": {}
						}
					},
					"Dependency.One.SubOne.SubSubTwo/1.0.0": {
						"runtime": {
							"Dependency.One.SubOne.SubSubTwo.dll": {}
						}
					},
					"Dependency.One.SubTwo/1.0.0": {
						"runtime": {
							"Dependency.One.SubTwo.dll": {}
						}
					}
				}
			}
		}
		"""

Scenario: Should not filter out any assemblies by defalt
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
		| --relation-level                                  |
		| none                                              |
		| --boundary-level                                  |
		| none                                              |
		| --title                                           |
		| not important                                     |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title not important
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubOne.SubSubOne/1.0.0", "Dependency.One.SubOne.SubSubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubOne.SubSubTwo/1.0.0", "Dependency.One.SubOne.SubSubTwo", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		
		"""

Scenario: Only included assemblies should end up as components
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
		| --include-assemblynames                           |
		| .*SubOne$;.*SubTwo$                               |
		| --relation-level                                  |
		| none                                              |
		| --boundary-level                                  |
		| none                                              |
		| --title                                           |
		| not important                                     |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title not important
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubOne.SubSubOne/1.0.0", "Dependency.One.SubOne.SubSubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubOne.SubSubTwo/1.0.0", "Dependency.One.SubOne.SubSubTwo", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		
		"""

Scenario: Excluded assemblies should not end up as components
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
		| --exclude-assemblynames                           |
		| .*SubOne$                                         |
		| --relation-level                                  |
		| none                                              |
		| --boundary-level                                  |
		| none                                              |
		| --title                                           |
		| not important                                     |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title not important
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		Component("Dependency.One.SubOne.SubSubTwo/1.0.0", "Dependency.One.SubOne.SubSubTwo", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		
		"""

Scenario: Exclude assemblies should win over include assemblies
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
		| --include-assemblynames                           |
		| .*SubOne$;.*SubTwo$                               |
		| --exclude-assemblynames                           |
		| .*SubOne$                                         |
		| --relation-level                                  |
		| none                                              |
		| --boundary-level                                  |
		| none                                              |
		| --title                                           |
		| not important                                     |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title not important
		Component("Dependency.One.SubOne.SubSubTwo/1.0.0", "Dependency.One.SubOne.SubSubTwo", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		
		"""
