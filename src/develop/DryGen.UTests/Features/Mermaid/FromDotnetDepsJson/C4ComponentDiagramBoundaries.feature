Feature: Generating Mermaid C4 Component diagram from .Net deps.json files with 'Container_Boundary' information

To understand the grouping of the dependencies for a .Net assembly
As a dry-gen user
I should be able to generate Mermaid C4 Component diagram from .Net deps.json file with 'Container_Boundary'v information

Scenario: Should create container boundary for dependencies by name split by '.'
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					},
					"Dependency.One/1.0.0": {
						"runtime": {
							"Dependency.One.dll": {}
						}
					},
					"Dependency.One.SubOne/1.0.0": {
						"runtime": {
							"Dependency.One.SubOne.dll": {}
						}
					},
					"Dependency.One.SubTwo/1.0.0": {
						"runtime": {
							"Dependency.One.SubTwo.dll": {}
						}
					},
					"Dependency.Two/1.0.0": {
						"runtime": {
							"Dependency.Two.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --include-exception-stacktrace                    |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Container_Boundary("Dependency", "Dependency") {
		Container_Boundary("Dependency.One", "Dependency.One") {
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		}
		Component("Dependency.Two/1.0.0", "Dependency.Two", "dll", "v1.0.0")
		}
		
		"""

Scenario: Should supress create container boundary when it only has one child
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					},
					"Dependency.One.SubOne/1.0.0": {
						"runtime": {
							"Dependency.One.SubOne.dll": {}
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
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		
		"""

Scenario: Should supress create container boundary when it only has only container boundary children
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					},
					"Dependency.One.SubOne/1.0.0": {
						"runtime": {
							"Dependency.One.SubOne.dll": {}
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
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Container_Boundary("Dependency.One", "Dependency.One") {
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		}
		
		"""
