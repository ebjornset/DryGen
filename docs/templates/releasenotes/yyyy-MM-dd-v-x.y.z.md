.!.!.replace-token-for-release-notes-header.!.!.
1. Fixed an issue that occurred when Mermaid diagrams are generated from EF Core when an input assebly referenced Asp.Net Core. 
   - Ie the .csproj file contains `<Project Sdk="Microsoft.NET.Sdk.Web">` or `<FrameworkReference Include="Microsoft.AspNetCore.App|All">` or references such an assembly.
2. .Net 7 is longer supported, since its no longer supported by Microsoft.
3. Switched to [docfx](https://dotnet.github.io/docfx/) for generating  the documentation. 
   - docfx uses .Net, so Ruby is no longer needed in the development toolchain

Have fun!