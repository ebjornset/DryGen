using System;
using System.IO;

namespace DryGen.DevUtils.Helpers;

public sealed class RootDirectoryContext : IDisposable
{
	public string RootDirectory { get; }

	public RootDirectoryContext()
	{
		RootDirectory = Path.Combine(Path.GetTempPath(), $"dry-gen-test-root-directory-{Guid.NewGuid()}");
	}

	public string BuldSubDirectory(Func<string, string> builder) {
		var result = builder(RootDirectory);
		Directory.CreateDirectory(result);
		return result;
	}

	public void Dispose()
	{
		DeleteRootDirectory();
		GC.SuppressFinalize(this);
	}

	private void DeleteRootDirectory()
	{
		if (Directory.Exists(RootDirectory))
		{
			Directory.Delete(RootDirectory, true);
		}
	}
}