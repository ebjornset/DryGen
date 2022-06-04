Feature: Replace magic token in output file

To be able to build documents with both non dry-gen generated information and dry-gen output
As a dry-gen user
I would like the functionality to replace a "magic token" in the output file with the generated representation instead of just overwrite the file

# This functionality is for every verb, but we only test it for 'options-from-commandline', since it executes quickest

Scenario: Should fail if 'replace-token-in-output-file' is specified, but 'output-file' is missing.
	When I call the program with this command line arguments
		| Arg                            |
		| options-from-commandline       |
		| --verb                         |
		| csharp-from-json-schema        |
		| --replace-token-in-output-file |
		| ignored-value                  |
	Then I should get exit code '1'
	And I should find the text "'replace-token-in-output-file' specified when 'output-file' is missing." in console error

Scenario: Should fail if 'replace-token-in-output-file' is specified, but the token is not found in the output file.
	Given this output file
	# The commandline argument -o <this filename> will be appended to the command line
		"""
		"""
	When I call the program with this command line arguments
		| Arg                            |
		| options-from-commandline       |
		| --verb                         |
		| csharp-from-json-schema        |
		| --replace-token-in-output-file |
		| not-found-value                |
	Then I should get exit code '1'
	And I should find the text "'replace-token-in-output-file' 'not-found-value' was not found in output file '" in console error

Scenario: Should generate options for verb csharp-from-json-schema as 'replace-token-in-output-file'
	Given this output file
	# The commandline argument -o <this filename> will be appended to the command line
		"""
		Some text before magic token
		found-value
		Some text after magic token
		"""
	When I call the program with this command line arguments
		| Arg                            |
		| options-from-commandline       |
		| --verb                         |
		| csharp-from-json-schema        |
		| --replace-token-in-output-file |
		| found-value                    |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		Some text before magic token
		#
		# dry-gen options for verb 'csharp-from-json-schema'
		#
		#array-instance-type: string
		#array-type: string
		#input-file: string
		#namespace: string
		#output-file: string
		#replace-token-in-output-file: string
		#root-classname: string
		#schema-file-format: byextension | json | yaml
		Some text after magic token
		"""
