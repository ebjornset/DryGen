﻿using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace DryGen;

internal class InternalAssemblyLoadContext : AssemblyLoadContext
{
	private readonly string inputFile;
	private readonly bool useAssemblyLoadContextDefault;
	private readonly string[] searchDirectories;

	private AssemblyLoadContext AssemblyLoadContext => useAssemblyLoadContextDefault ? Default : this;

	internal InternalAssemblyLoadContext(string inputFile, bool useAssemblyLoadContextDefault, AspNetCoreSharedFolderResolver aspNetCoreSharedFolderResolver)
	{
		this.inputFile = inputFile;
		this.useAssemblyLoadContextDefault = useAssemblyLoadContextDefault;
		var inputDirectory = Path.GetDirectoryName(inputFile) ?? throw new OptionsException($"Could not determine directory from inputFile '{inputFile}'");
		var aspNetCoreDirectoryApp = aspNetCoreSharedFolderResolver.Resolve();
		searchDirectories = string.IsNullOrEmpty(aspNetCoreDirectoryApp)
			? [inputDirectory, aspNetCoreSharedFolderResolver.DotNetRuntimeDirectory]
			: [inputDirectory, aspNetCoreDirectoryApp, aspNetCoreSharedFolderResolver.DotNetRuntimeDirectory];
	}

	internal Assembly Load()
	{
		// Seems like we cant put a using on the ContextualReflectionScope returned from EnterContextualReflection(),
		// since the protected Load never gets called if we do and then we can't load any dependencies.
		AssemblyLoadContext.EnterContextualReflection();
		return LoadFromFile(inputFile);
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		foreach (var extension in new[] { ".dll", ".exe" })
		{
			foreach (var loadDirectory in searchDirectories)
			{
				var assemblyFileName = $"{loadDirectory}{Path.DirectorySeparatorChar}{assemblyName.Name}{extension}";
				if (File.Exists(assemblyFileName))
				{
					return LoadFromFile(assemblyFileName);
				}
			}
		}
		// We can't find the assembly file, let the runtime try to handle it
		return null;
	}

	private Assembly LoadFromFile(string assemblyFileName)
	{
		// It seems like LoadFromAssemblyPath lockes the file, and that makes our teste fail, so we read the file manually to a stream
		var assemblyBytes = File.ReadAllBytes(assemblyFileName);
		return AssemblyLoadContext.LoadFromStream(new MemoryStream(assemblyBytes));
	}
}