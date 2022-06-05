Feature: Filtering by namspace

To be able to generate Mermaid digrams from the interesting parts of the C# code
As a dry-gen user
I should be able to include and types by their name space using regex

Background:
	Given this C# source code
		"""
		namespace Test.One
		{
			public class Customer {}
		}
		namespace Test.Two
		{
			public class Order {}
			public class OrderLine {}
		}
		namespace Test.Three
		{
			public class Product {}
			public class ProductType {}
			public class ProductImage {}
		}
		"""

Scenario: Includes all namespaces by default
	When I load the types to include in the diagram
	Then I get this list of types:
		| Namespace  | Name         |
		| Test.One   | Customer     |
		| Test.Two   | Order        |
		| Test.Two   | OrderLine    |
		| Test.Three | Product      |
		| Test.Three | ProductType  |
		| Test.Three | ProductImage |
		| System     | Object       |

Scenario: Should remove namespaces not included
	Given this include namespace filter '<Regex>'
	When I load the types to include in the diagram
	Then I get '<Count>' types
Examples:
	| Regex    | Count | Comment                |
	| .*       | 7     | Includes System.Object |
	| ^Test.*  | 6     |                        |
	| .*One$   | 1     |                        |
	| .*Two$   | 2     |                        |
	| .*Three$ | 3     |                        |

Scenario: Should remove namespaces not included in at least one regex
	Given these include namespace filters
		| Regex    |
		| <Regex1> |
		| <Regex2> |
	When I load the types to include in the diagram
	Then I get '<Count>' types
Examples:
	| Regex1 | Regex2   | Count | Comment                |
	| .*     | NoMatch  | 7     | Includes System.Object |
	| .*One$ | NoMatch  | 1     |                        |
	| .*Two$ | .*Three$ | 5     |                        |

