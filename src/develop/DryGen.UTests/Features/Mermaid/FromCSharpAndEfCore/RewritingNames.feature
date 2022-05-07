Feature: Rewriting names

A short summary of the feature

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
