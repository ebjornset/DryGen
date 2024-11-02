.!.!.replace-token-for-release-notes-header.!.!.
> [!IMPORTANT]
> In v 2.x .Net 6 and .Net 7 will no longer be supported, since they're no longer supported by Microsoft.

### Improvements in this version
## Prerelease 2
- Added support for generating many to many associations in `mermaid-class-diagram-from-csharp`.
- Added support for generating many to many relations in `mermaid-er-diagram-from-csharp`.
- Added support for `include-exception-stacktrace` in `verbs-from-options-file`.
## Prerelease 1 (2024-10-29)
- Fixed an issue where  `mermaid-er-diagram-from-efcore` would fail when an input assebly referenced Asp.Net Core. 
   - Ie the .csproj file contained `<Project Sdk="Microsoft.NET.Sdk.Web">` or `<FrameworkReference Include="Microsoft.AspNetCore.App|All">` or referenced such an assembly.
- Added support for generating many to many relations in `mermaid-er-diagram-from-efcore`.
- Switched to [docfx](https://dotnet.github.io/docfx/) for generating  the documentation. 
   - docfx uses .Net, so Ruby is no longer needed in the development toolchain

Have fun!