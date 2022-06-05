Feature: Rewriting names

To be able to generate Mermaid digrams that uses the domain language, even when the C# code follos a naming convention that is using technical terms
As a dry-gen user
I should be able to rewrite the C# type name to match the domain language, e.g by removing an Entity suffix from all EF Core entities


Scenario: Does not rewrite names by default
	When I rewrite this list of names:
		| Value    |
		| Customer |
		| Order    |
		| Product  |
	Then I get this list of names:
		| Value    |
		| Customer |
		| Order    |
		| Product  |

Scenario: Does rewrite names by replace
	Given this name replase string '<Replace>' and replacement '<Replacement>'
	When I rewrite the name '<Name>'
	Then I get the name '<Replaced name>'
Examples:
	| Replace | Replacement | Name           | Replaced name |
	|         |             | Test           | Test          |
	| Entity  |             | TestEntity     | Test          |
	| Entity  |             | EntityTest     | Test          |
	| Test    |             | TestEntityTest | Entity        |
