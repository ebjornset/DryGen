Feature: Filtering by type name

A short summary of the feature

Background:
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer {}
			public class Order {}
			public class OrderLine {}
			public class Product {}
			public class ProductType {}
			public abstract class AbstractProduct {}
		}
		"""

Scenario: Includes all type names by default
	When I load the types to include in the diagram
	Then I get this list of types:
		| Namespace | Name            |
		| Test      | Customer        |
		| Test      | Order           |
		| Test      | OrderLine       |
		| Test      | Product         |
		| Test      | ProductType     |
		| Test      | AbstractProduct |
		| System    | Object          |

Scenario: Should remove type names excluded
	Given this exclude type name filter '<Regex>'
	When I load the types to include in the diagram
	Then I get '<Count>' types
Examples:
	| Regex       | Count |
	| Object      | 6     |
	| ^Product.*  | 5     |
	| .*Product.* | 4     |
	| .*Order$    | 6     |

Scenario: Should remove type names excluded in at least one regex
	Given these exclude type name filters
		| Regex    |
		| <Regex1> |
		| <Regex2> |
	When I load the types to include in the diagram
	Then I get '<Count>' types
Examples:
	| Regex1     | Regex2   | Count |
	| .*         | NoMatch  | 0     |
	| ^Product.* | NoMatch  | 5     |
	| ^Product.* | .*Order$ | 4     |


Scenario: Should keep type names included
	Given this include type name filter '<Regex>'
	When I load the types to include in the diagram
	Then I get '<Count>' types
Examples:
	| Regex       | Count |
	| ^Product.*  | 2     |
	| .*Product.* | 3     |
	| .*Order$    | 1     |

Scenario: Should keep type names included in at least one regex
	Given these include type name filters
		| Regex    |
		| <Regex1> |
		| <Regex2> |
	When I load the types to include in the diagram
	Then I get '<Count>' types
Examples:
	| Regex1     | Regex2   | Count |
	| .*         | NoMatch  | 7     |
	| ^Product.* | NoMatch  | 2     |
	| ^Product.* | .*Order$ | 3     |
