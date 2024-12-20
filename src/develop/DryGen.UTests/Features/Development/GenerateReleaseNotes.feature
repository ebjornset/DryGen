﻿Feature: Generate release notes from templates

To be able to produce a consistent release notes layout
As a dry-gen developer
I should be able to generate the release notes header and the toc from the release templates

Scenario: Should generate table of context for release notes
	Given the release notes template folder contains these files
		| File name             |
		| 2022-09-03-v-0.7.1.md |
		| 2023-02-22-v-1.0.0.md |
		| 2024-04-07-v-1.6.2.md |
		| 2024-11-03-v-2.0.0-prerelease0002.md |
	When I generate the release notes TOC
	Then I should not get an exception
	And console out should contain the text
		"""
		# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		- name: Version 2.0.0-prerelease0002
		  href: v-2.0.0-prerelease0002.md
		- name: Version 1.6.2
		  href: v-1.6.2.md
		- name: Version 1.0.0
		  href: v-1.0.0.md
		- name: Version 0.7.1
		  href: v-0.7.1.md
		
		"""

Scenario: Should copy release notes file from template and replace header
	Given the release notes template folder contains the file "2023-10-31-v-1.3.1.md" with content
		"""
		.!.!.replace-token-for-release-notes-header.!.!.
		Markdown content...
		"""
	When I generate the release notes file "2023-10-31-v-1.3.1.md"
	Then I should not get an exception
	And the release notes folder should contain the file "v-1.3.1.md" with content
		"""
		<!--
		Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		Source file: "docs/templates/releasenotes/2023-10-31-v-1.3.1.md"
		-->
		# Version 1.3.1
		**Release date 2023-10-31**
		
		Markdown content...
		"""

Scenario: Should only accept valid release notes file names
	Given today is "2024-04-28"
	And the release notes template folder contains these files
		| File name             |
		| yyyy-MM-dd-v-x.y.z.md |
		| <File name>           |
	When I generate the release notes TOC
	Then I should get an exception containing the text "<Exception text>"
Examples:
	| File name                 | Exception text                                                                                                                                                                                                                                          |
	| x.md                      | 'x.md': The file name must be 'yyyy-MM-dd-v-x.y.z.md' or on the format '<yyyy-MM-dd>-v-<x.y.z>.md', where <yyyy-MM-dd> is a date (NB! Not in the future) and <x.y.z> is a valid version number!                                                         |
	| 2024-04-28_v-x.y.z.md     | '2024-04-28_v-x.y.z.md': The file name must be 'yyyy-MM-dd-v-x.y.z.md' or on the format '<yyyy-MM-dd>-v-<x.y.z>.md', where <yyyy-MM-dd> is a date (NB! Not in the future) and <x.y.z> is a valid version number!                                        |
	| 2024-04-28-V-x.y.z.md     | '2024-04-28-V-x.y.z.md': The file name must be 'yyyy-MM-dd-v-x.y.z.md' or on the format '<yyyy-MM-dd>-v-<x.y.z>.md', where <yyyy-MM-dd> is a date (NB! Not in the future) and <x.y.z> is a valid version number!                                        |
	| 2024-04-28-v_x.y.z.md     | '2024-04-28-v_x.y.z.md': The file name must be 'yyyy-MM-dd-v-x.y.z.md' or on the format '<yyyy-MM-dd>-v-<x.y.z>.md', where <yyyy-MM-dd> is a date (NB! Not in the future) and <x.y.z> is a valid version number!                                        |
	| 2024-31-01-v-1.2.3.md     | '2024-31-01-v-1.2.3.md': The file name must be 'yyyy-MM-dd-v-x.y.z.md' or on the format '<yyyy-MM-dd>-v-<x.y.z>.md', where <yyyy-MM-dd> is a date (NB! Not in the future) and <x.y.z> is a valid version number!                                        |
	| 2024-04-29-v-1.2.3.md     | '2024-04-29-v-1.2.3.md': '2024-04-29' is in the future                                                                                                                                                                                                  |
	| 2024-04-28-v-1.2.z.md     | '2024-04-28-v-1.2.z.md': '1.2.z' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.y.3.md     | '2024-04-28-v-1.y.3.md': '1.y.3' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-x.2.3.md     | '2024-04-28-v-x.2.3.md': 'x.2.3' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.3.4.md   | '2024-04-28-v-1.2.3.4.md': '1.2.3.4' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.a-3.4.md | '2024-04-28-v-1.2.a-3.4.md': '1.2.a-3.4' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.3.4.5.md | '2024-04-28-v-1.2.3.4.5.md': '1.2.3.4.5' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.3-alpha007.md | '2024-04-28-v-1.2.3-alpha007.md': '1.2.3-alpha007' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.3-alpha00007.md | '2024-04-28-v-1.2.3-alpha00007.md': '1.2.3-alpha00007' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.3-alpha.0007.md | '2024-04-28-v-1.2.3-alpha.0007.md': '1.2.3-alpha.0007' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
	| 2024-04-28-v-1.2.3-alpha-0007.md | '2024-04-28-v-1.2.3-alpha-0007.md': '1.2.3-alpha-0007' is not a valid version number. It must be on the format a[.b[.c[-<prerelease name>dddd]]], where a, b and c are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease', and dddd is a four digit integer |
