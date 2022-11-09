---
# .!.!.replace-token-for-autogenerated-by-docs-warning.!.!.
# .!.!.replace-token-for-autogenerated-by-docs-source.!.!.
layout: page
title: Examples of how to control the Mermaid Class diagram detail level
description: Examples of how to control the Mermaid Class diagram detail level
show_sidebar: false
toc: true
menubar: examples_menu
hero_height: is-fullwidth
---
These examples shows how you can control the property and method information from your C# types that's included in your Mermaid Class diagrams. Most of the examples uses one singe dry-gen C# type, the ClassDiagramGenerator, to make it easier to see the effect of the example's option. 

{% include notification.html status="is-dark" 
message="The option `--exclude-propertynames` works the same way for Mermaid class diagram as for Er diagrams. Look at the example in the [Mermaid ER diagram details examples](../mermaid-er-diagram-details/) for details." %}
### Example one: No filtering
Sometimes a C# type can contain quite a few methods and or properties, and thus can look quite complex in a Mermaid class diagram.
#### The commandline
`.!.!.replace-token-for-mermaid-class-diagram-details-example-no-filtering-commandline.!.!.`
#### The resulting Mermaid diagram
```mermaid
.!.!.replace-token-for-mermaid-class-diagram-details-example-no-filtering.!.!.
```
### Example two: Make more high level diagrams with `--method-level` and `--attribute-level`
If you want a more high level diagram, you can suppress methods by their visibility with `--method-level`. 

{% include notification.html status="is-dark" 
message="Use `--attribute-level` to control what C# properties to include in your diagram." %} 
&nbsp; 

In this example we only show the public methods. 

#### The commandline
`.!.!.replace-token-for-mermaid-class-diagram-details-example-method-level-commandline.!.!.`
#### The resulting Mermaid diagram
```mermaid
.!.!.replace-token-for-mermaid-class-diagram-details-example-method-level.!.!.
```
### Example three: Focus on the domain's structure with `--exclude-static-methods` and `--exclude-static-attributes`
Sometimes your class diagram are all about the structure of a domain. Since static methods often tends to be "helper methods", they might not give any extra information about the structure. In these situations it might be useful to supress them with `--exclude-static-methods`. 

{% include notification.html status="is-dark" 
message="You can also exclude any static attibutes with `--exclude-static-attributes`, but these tends to be seldomly used." %} 
&nbsp;
#### The commandline
`.!.!.replace-token-for-mermaid-class-diagram-details-example-exclude-static-methods-commandline.!.!.`
#### The resulting Mermaid diagram
```mermaid
.!.!.replace-token-for-mermaid-class-diagram-details-example-exclude-static-methods.!.!.
```
### Example four: Simplify method signatures with `--exclude-method-params`
Use `--exclude-method-params` if you want a simpler diagram without details about all the methods parameters. 

#### The commandline
`.!.!.replace-token-for-mermaid-class-diagram-details-example-exclude-method-params-commandline.!.!.`
#### The resulting Mermaid diagram
```mermaid
.!.!.replace-token-for-mermaid-class-diagram-details-example-exclude-method-params.!.!.
```
### Example five: Hiding naming conventions with `--name-replace-from` and `--name-replace-to`
Sometimes your C# code follow some naming convention where a lot of your types ends up with the same prefix or post fix, e.g. `.*Entity$` for all your types stored in the database by Entity Framework, or like in dry.gen where all types related to generation of Mermaid class diagrams has the convention `^ClassDiagram.*`. You might want to hide this convention from your resulting diagram, to focus on the pure domain concepts. 

In this example we hides the  `^ClassDiagram.*` convention for all the  `^ClassDiagram.*` C# types in dry.gen. 

{% include notification.html status="is-dark" 
message="Here we only also exclude all attibutes and methods with `attribute-level none --method-level none`, to make the resulting diagram more focused on the class names" %} 
&nbsp;
#### The commandline
`.!.!.replace-token-for-mermaid-class-diagram-details-example-name-replace-commandline.!.!.`
#### The resulting Mermaid diagram
```mermaid
.!.!.replace-token-for-mermaid-class-diagram-details-example-name-replace.!.!.
```
{% include convert-fenced-mermaid-code-blocks-to-mermaid-div-script.html %}