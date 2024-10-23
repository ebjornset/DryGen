.!.!.replace-token-for-release-notes-header.!.!.
- Fixed an issue that occurred when generating Mermaid ER diagrams from EF Core when an input assebly referenced Asp.Net Core. 
   - Ie the .csproj file contains `<Project Sdk="Microsoft.NET.Sdk.Web">` or `<FrameworkReference Include="Microsoft.AspNetCore.App|All">` or references such an assembly.
- Added support for generating many to many relations in `mermaid-er-diagram-from-efcore`.
- .Net 7 is longer supported, since its no longer supported by Microsoft.
- Switched to [docfx](https://dotnet.github.io/docfx/) for generating  the documentation. 
   - docfx uses .Net, so Ruby is no longer needed in the development toolchain

Have fun!