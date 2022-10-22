Feature: Extension methods as instance methods in Class diagrams

To be able to generate Class diagrams that logically matches my code
As a dry-gen user
I would like extension methods to be instance methods on the extended classes in my Class diagrams

Scenario: Extension methods for known type should be instance methods on the extended type
	Given this C# source code
		"""
		namespace Test
		{
			public class Implementation {
			}
			public static class Extensions {
				public static void ExtensionMethodAsImplementation(this Implementation implementation) {}
				public static void NotAnExtensionMethod(Implementation implementation) {}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Extensions {
				<<abstract>>
				+NotAnExtensionMethod(Implementation implementation)$
			}
			class Implementation {
				+ExtensionMethodAsImplementation()
			}

		"""

Scenario: Extension methods for unknown types should be static methods in the extension type
	Given this C# source code
		"""
		namespace Test
		{
			public static class Extensions {
				public static void ExtensionMethodOnUnkownType(this string unknownType) {}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Extensions {
				<<abstract>>
				+ExtensionMethodOnUnkownType(string unknownType)$
			}

		"""

Scenario: Static classes with only extension methods for known types should be excluded from the diagram
	Given this C# source code
		"""
		namespace Test
		{
			public class Implementation {
			}
			public static class Extensions {
				public static void ExtensionMethodAsImplementation(this Implementation implementation) {}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Implementation {
				+ExtensionMethodAsImplementation()
			}

		"""

Scenario: Extension methods of all legal visibilities should be instance methods in the extended type
	Given this C# source code
		"""
		namespace Test
		{
			public class Implementation {
			}
			public static class Extensions {
				public static void PublicExtensionMethod(this Implementation implementation) {}
				internal static void InternalExtensionMethod(this Implementation implementation) {}
				// Protected methods are not legal in static classes
				private static void PrivateExtensionMethod(this Implementation implementation) {}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Implementation {
				+PublicExtensionMethod()
				~InternalExtensionMethod()
				-PrivateExtensionMethod()
			}

		"""

Scenario: Extension methods for fitered out types should be static methods in the extension type
	Given this C# source code
		"""
		namespace Test
		{
			public class Implementation {
			}
			public static class Extensions {
				public static void ExtensionMethod(this Implementation implementation) {}
			}
		}
		"""
	And this exclude type name filter 'Implementation'
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Extensions {
				<<abstract>>
				+ExtensionMethod(Implementation implementation)$
			}

		"""
