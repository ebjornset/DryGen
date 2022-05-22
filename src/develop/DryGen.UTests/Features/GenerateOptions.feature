Feature: Generate options from command line

To be able to use option files without having to hand crafdt them from scratch
As a dry-gen user
I would like to be able to generate option files for all verbs

Scenario: Should generate options for verb options-from-commandline
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
		| --verb                   |
		| options-from-commandline |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		"""
