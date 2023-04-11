@notParallel
Feature: Generating Mermaid C4 Component diagram from .Net deps.json files with 'techn' information

To understand the .Net version of the dependencies for a .Net assembly
As a dry-gen user
I should be able to generate Mermaid C4 Component diagram from .Net deps.json file with techn information

Scenario: Should select 'native' as 'techn' when it's present in depts.json file with child content
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
				},
				".NETCoreApp,Version=v7.0/win-x64": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"MainAssembly.dll": {}
						},
						"native": {
							"MainAssembly.Native.amd64.dll": {
								"fileVersion": "1.0.0"
							}
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0/win-x64
		Component("MainAssembly/1.0.0", "MainAssembly", "native", "v1.0.0")
		
		"""

Scenario: Should select runtime 'lib/' as 'techn' when 'native' has no content and another slash ('/') is found 
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
				},
				".NETCoreApp,Version=v7.0/win-x64": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"lib/net7.0/MainAssembly.dll": {}
						},
						"native": {
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0/win-x64
		Component("MainAssembly/1.0.0", "MainAssembly", "net7.0", "v1.0.0")
		
		"""

Scenario: Should select runtime extension as 'techn' when 'lib/' as no next slash ('/') and runtime has extension
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
				},
				".NETCoreApp,Version=v7.0/win-x64": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"lib/MainAssembly.dll": {}
						},
						"native": {
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0/win-x64
		Component("MainAssembly/1.0.0", "MainAssembly", "dll", "v1.0.0")
		
		"""

Scenario: Should select empty string  as 'techn' as a last resort
	Given this .Net depts json input file
		"""
		{
			"targets": {
				".NETCoreApp,Version=v7.0": {
				},
				".NETCoreApp,Version=v7.0/win-x64": {
					"MainAssembly/1.0.0": {
						"runtime": {
							"lib/MainAssemblyWiothoutExtension": {}
						},
						"native": {
						}
					}
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                                               |
		| mermaid-c4component-diagram-from-dotnet-deps-json |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		C4Component
		title Component diagram for MainAssembly v1.0.0 running on .NETCoreApp v7.0/win-x64
		Component("MainAssembly/1.0.0", "MainAssembly", "", "v1.0.0")
		
		"""
