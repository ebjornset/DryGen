# Verbs
The menu on the left lists all the dry-gen verbs. Each of these pages will give you an overview of the verb's options and an options file template for the verb.

>[!TIP]
>You can always get the information about the options for a verb by running the command
>
>`dry-gen <the verb> --help`

>[!TIP]
>You can also generate an options file template for a verb by running the command
>
>`dry-gen options-from-commandline --verb <the verb>`

>[!TIP]
>You can reference environment variables in the options files, using the MSBuild syntax
`$(<environment variable name>)`
>
>The environment variable names are not case sesitive.  
>If the environment variable doesn't exists, it's replaced with an empty string instead.

>[!NOTE]
> Due to a limitation in the library we use for commandline parsing, all boolean options must be specified as `--<boolean option> true|false`