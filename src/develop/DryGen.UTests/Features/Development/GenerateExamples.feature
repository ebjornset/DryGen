﻿Feature: Generate examples of usage

To be able to showcase our features
As a dry-gen developer
I should be able to generate examples by running the tool using our own codebase as the source representation

Scenario: Should generate TOC for all examples
	Given the examples template folder contains these files
		| File name                           |
		| Mermaid-Er_diagrams-from-EF_Core.md |
		| Filter-examples.md                  |
		| Mermaid-Class_diagrams.md           |
	When I generate the examples TOC
	Then console out should contain the text
		"""
		# Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		- name: Overview
		  href: index.md
		- name: Filter examples
		  href: filter-examples.md
		- name: Mermaid Class diagrams
		  href: mermaid-class_diagrams.md
		- name: Mermaid Er diagrams from EF Core
		  href: mermaid-er_diagrams-from-ef_core.md
		
		"""

Scenario: Should copy example file from template and replace autogenerated by docs warning token
	Given the examples template folder contains the file "Mermaid-Class_diagrams.Md" with content
		"""
		<!--
		.!.!.replace-token-for-autogenerated-by-docs-warning.!.!.
		.!.!.replace-token-for-autogenerated-by-docs-source.!.!.
		-->
		Markdown content...
		"""
	When I generate the examples file "Mermaid-Class_diagrams.Md"
	Then the examples folder should contain the file "mermaid-class_diagrams.md" with content
		"""
		<!--
		Autogenerated by the Docs build step. Do not edit this file by hand, as your edits will be overwritten by the next Docs build.
		Source file: "docs/templates/examples/Mermaid-Class_diagrams.Md"
		-->
		Markdown content...
		"""

# We could test for the generation of more examples, but that would only make more fragile test, since the examples changes each time the examples description or the code base changes...