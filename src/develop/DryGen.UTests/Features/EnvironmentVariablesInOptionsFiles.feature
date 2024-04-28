Feature: Environment variables in options files

To be able to reuse option sets between different folder locations
As a dry-gen user
I should be able to use environment variables in my options files

Scenario: Know environment variable should be replaced with its value
	Given the environment variable "verb" has the value "options-from-commandline"
	And this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		verb: $(verb)
		"""
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
	Then I should get exit code '0'
	And console out should contain the text
		"""
		#
		# dry-gen options for verb 'options-from-commandline'
		#
		#output-file: string
		#output-template: string
		#replace-token-in-output-file: string
		#verb: string
		"""

Scenario: Only complete environment variable fragments should be replaced
	Given the environment variable "verb" has the value "options-from-commandline"
	And this content as an options file
	# The commandline arguments -f <this filename> will be appended to the command line
		"""
		verb: <Fragment>
		"""
	When I call the program with this command line arguments
		| Arg                      |
		| options-from-commandline |
	Then I should get exit code '<Exit code>'
Examples:
	| Fragment  | Exit code |
	| $(verb    | 1         |
	| verb)     | 1         |
	| $(verb)   | 0         |
