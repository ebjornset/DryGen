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
	# We must use two levels of boundries in this test, and can only suppress the deepest, 
	# since a single dependency on the first level will get captured by the syntetic 'Standalone dependencies' boundary.
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
					"Dependency.One.SubOne.Sub/1.0.0": {
						"runtime": {
							"Dependency.One.SubOne.Sub.dll": {}
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
		Container_Boundary("Dependency.One.SubOne", "Dependency.One.SubOne") {
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubOne.Sub/1.0.0", "Dependency.One.SubOne.Sub", "dll", "v1.0.0")
		}
		
		"""

Scenario: Should supress container boundary when it only has container boundaries as children
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

Scenario: Should group standalone dependencies in a synthetic container boundary 'Standalone dependencies'
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
					"DependencyOne/1.0.0": {
						"runtime": {
							"DependencyOne.dll": {}
						}
					},
					"DependencyTwo/1.0.0": {
						"runtime": {
							"DependencyTwo.dll": {}
						}
					},
					"DependencyThree/1.0.0": {
						"runtime": {
							"DependencyThree.dll": {}
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
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.0.0", "DependencyOne", "dll", "v1.0.0")
		Component("DependencyTwo/1.0.0", "DependencyTwo", "dll", "v1.0.0")
		Component("DependencyThree/1.0.0", "DependencyThree", "dll", "v1.0.0")
		}
		
		"""

Scenario: Should not supress synthetic container boundary 'Standalone dependencies'
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
					"DependencyOne/1.0.0": {
						"runtime": {
							"DependencyOne.dll": {}
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
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.0.0", "DependencyOne", "dll", "v1.0.0")
		}
		
		"""
