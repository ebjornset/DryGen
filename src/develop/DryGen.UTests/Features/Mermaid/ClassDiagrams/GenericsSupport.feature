Feature: Generics support in Class diagrams

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates class for generic type
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class Shape {}
			public class Square<Shape> {}
			public class Squares<T> where T : IEnumerable<IEnumerable<Shape>> {}
		}
		"""
	When I generate a Class diagram
	Then I should get this Mermaid code
	# Mermaid only support one level of generics, so the <> in any nested generic types are replaced with "Of" as a workaround
		"""
		classDiagram
			class Shape {
			}
			class Square~Shape~ {
			}
			class Squares~IEnumerableOfIEnumerableOfShape~ {
			}
		
		"""

Scenario: Generates dependency for all generic constructor arguments with known types as generic parameter
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A {
				public A(IEnumerable<IEnumerable<B>> bs, IDictionary<C,D> cds) {} 
			}
			public class B {}
			public class C {}
			public class D {}
		}
		"""
	When I generate a Class diagram
	Then I should get this Mermaid code
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			class D {
			}
			A ..> B
			A ..> C
			A ..> D
		
		"""

Scenario: Generates data types for all generic properties
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A {
				public IDictionary<int, IDictionary<bool, IDictionary<int,string>>> GenericProperty { get; set; } 
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this Mermaid code
	# Mermaid only support one level of generics, so the <> in any nested generic types are replaced with "Of" as a workaround
		"""
		classDiagram
			class A {
				+IDictionary~int,IDictionaryOfbool,IDictionaryOfint,string~ GenericProperty
			}
		
		"""

Scenario: Generates data types for all generic method params and return types
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A {
				public IDictionary<int, IDictionary<bool,IDictionary<int,string>>> GenericMethod(IDictionary<float, IDictionary<int, bool>> genericParam) { return null; } 
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this Mermaid code
	# Mermaid only support one level of generics, so the <> in any nested generic types are replaced with "Of" as a workaround
		"""
		classDiagram
			class A {
				+GenericMethod(IDictionary~float,IDictionaryOfint,bool~ genericParam) IDictionary~int,IDictionaryOfbool,IDictionaryOfint,string~
			}
		
		"""
