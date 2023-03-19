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
					},
					"DependencyOne/1.1.0": {
						"runtime": {
							"DependencyOne.dll": {}
						}
					},
					"DependencyTwo/1.2.0": {
						"runtime": {
							"DependencyTwo.dll": {}
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
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.1.0", "DependencyOne", "dll", "v1.1.0")
		Component("DependencyTwo/1.2.0", "DependencyTwo", "dll", "v1.2.0")
		}
		
		"""

Scenario: Should not include any 'Rel' when '--relations-level' is 'none'
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
					},
					"DependencyTwo/1.2.0": {
						"runtime": {
							"DependencyTwo.dll": {}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4container-diagram-from-dotnet-deps-json |
		| --relations-level                                 |
		| none                                              |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		Container_Boundary("Standalone dependencies", "Standalone dependencies") {
		Component("DependencyOne/1.1.0", "DependencyOne", "dll", "v1.1.0")
		Component("DependencyTwo/1.2.0", "DependencyTwo", "dll", "v1.2.0")
		}
		
		"""

Scenario: Should not include any 'Container_Boundary' when '--boundaries-level' is 'none'
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
		| --boundaries-level                                |
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
		Component("Dependency.Two/1.0.0", "Dependency.Two", "dll", "v1.0.0")
		
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
					},
					"DependencyTwo/1.2.0": {
						"runtime": {
							"DependencyTwo.dll": {}
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
		Component("DependencyTwo/1.2.0", "DependencyTwo", "dll", "")
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
					},
					"DependencyTwo/1.2.0": {
						"runtime": {
							"lib/net6.0/DependencyTwo.dll": {}
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
		Component("DependencyTwo/1.2.0", "DependencyTwo", "", "v1.2.0")
		}
		
		"""
