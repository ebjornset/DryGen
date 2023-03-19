Feature: Controlling the generation of Mermaid C4 Component diagram from .Net deps.json files with options

To be able to control the content of the generated diagram
As a dry-gen user
I should be able to control how the Mermaid C4 Component diagram is generated from .Net deps.json file with options

Scenario: Should use '--title'
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --title                                           |
		| Custom title                                      |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Custom title
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		
		"""

Scenario: Should not include any 'Rel' when '--relation-level' is 'none'
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"dependencies": {
							"DependencyOne": "1.1.0",
							"DependencyTwo": "1.2.0",
						},
						"runtime": {
							"MainAssembly.dll": {}
						}
					},
					"DependencyOne/1.1.0": {
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
		| --relation-level                                 |
		| none                                              |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.1.0", "DependencyOne", "dll", "v1.1.0")
		}
		
		"""

Scenario: Should include 'Internal assemblies' and 'External dependencies' as 'Container_Boundary' when '--boundary-level' is 'internalexternal'
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly.Cli/1.0.0": {
						"runtime": {
							"MainAssembly.Cli.dll": {}
						}
					},
					"MainAssembly.Core/1.0.0": {
						"runtime": {
							"MainAssembly.Core.dll": {}
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
		| --boundary-level                                |
		| internalexternal                                  |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly.Cli v1.0.0 running on .NETCoreApp v7.0
		Container_Boundary("Internal assemblies", "Internal assemblies") {
		Component("MainAssembly.Cli/1.0.0", "MainAssembly.Cli", "dll", "v1.0.0")
		Component("MainAssembly.Core/1.0.0", "MainAssembly.Core", "dll", "v1.0.0")
		}
		Container_Boundary("External dependencies", "External dependencies") {
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		Component("Dependency.Two/1.0.0", "Dependency.Two", "dll", "v1.0.0")
		}
		
		"""

Scenario: Should not include 'Internal assemblies' as 'Container_Boundary' when '--boundary-level' is 'internalexternal', but we oonly have one internal assembly
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
		| --boundary-level                                |
		| internalexternal                                  |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Container_Boundary("External dependencies", "External dependencies") {
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		Component("Dependency.Two/1.0.0", "Dependency.Two", "dll", "v1.0.0")
		}
		
		"""

Scenario: Should not include 'External dependencies' as 'Container_Boundary' when '--boundary-level' is 'internalexternal', but we have only one externa dependency
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly.Cli/1.0.0": {
						"runtime": {
							"MainAssembly.Cli.dll": {}
						}
					},
					"MainAssembly.Core/1.0.0": {
						"runtime": {
							"MainAssembly.Core.dll": {}
						}
					},
					"Dependency.One/1.0.0": {
						"runtime": {
							"Dependency.One.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --boundary-level                                |
		| internalexternal                                  |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly.Cli v1.0.0 running on .NETCoreApp v7.0
		Container_Boundary("Internal assemblies", "Internal assemblies") {
		Component("MainAssembly.Cli/1.0.0", "MainAssembly.Cli", "dll", "v1.0.0")
		Component("MainAssembly.Core/1.0.0", "MainAssembly.Core", "dll", "v1.0.0")
		}
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		
		"""

Scenario: Should not include any 'Container_Boundary' when '--boundary-level' is 'none'
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
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --boundary-level                                |
		| none                                              |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Component("Dependency.One/1.0.0", "Dependency.One", "dll", "v1.0.0")
		Component("Dependency.One.SubOne/1.0.0", "Dependency.One.SubOne", "dll", "v1.0.0")
		Component("Dependency.One.SubTwo/1.0.0", "Dependency.One.SubTwo", "dll", "v1.0.0")
		
		"""

Scenario: Should not include version numbers in description when '--exclude-version' is 'true'
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
					"DependencyOne/1.1.0": {
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
		| --exclude-version                                 |
		| true                                              |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "")
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.1.0", "DependencyOne", "dll", "")
		}
		
		"""

Scenario: Should not include techn when '--exclude-techn' is 'true'
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
					"DependencyOne/1.1.0": {
						"runtime": {
							"lib/net7.0/DependencyOne.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --exclude-techn                                   |
		| true                                              |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "", "v1.0.0")
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.1.0", "DependencyOne", "", "v1.1.0")
		}
		
		"""

Scenario: Should use '--shape-in-row' as '$c4ShapeInRow' in 'UpdateLayoutConfig'
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --shape-in-row                                    |
		| 7                                                 |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		UpdateLayoutConfig($c4ShapeInRow = "7", $c4BoundaryInRow = "2")
		
		"""

Scenario: Should use '--boundary-in-row' as '$c4ShapeInRow' in 'UpdateLayoutConfig'
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --boundary-in-row                                 |
		| 7                                                 |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		UpdateLayoutConfig($c4ShapeInRow = "4", $c4BoundaryInRow = "7")
		
		"""
