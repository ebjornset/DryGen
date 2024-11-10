using System.IO;
using System.Linq;
using DryGen.Core;

namespace DryGen;

public class AspNetCoreSharedFolderResolver(string dotNetRuntimeDirectory)
{
	public string DotNetRuntimeDirectory => dotNetRuntimeDirectory.AsLinuxPath().TrimEnd('/');

	public AspNetCoreSharedFolderResolver() : this(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory())
	{
	}

	public string? Resolve()
	{
		var result = DotNetRuntimeDirectory.Replace("Microsoft.NETCore.App", "Microsoft.AspNetCore.App");
		if (Directory.Exists(result))
		{
			return result.AsLinuxPath();
		}
		var dotNetRuntimeVersion = new DirectoryInfo(result).Name;
		var baseAspNetDirectory = new DirectoryInfo(result[..result.LastIndexOf('/')]).AsNonNull();
		var bestCandidate = baseAspNetDirectory.GetDirectories().Where(x => IsCandidate(x.Name, dotNetRuntimeVersion)).OrderByDescending(x => x.Name).FirstOrDefault();
		return bestCandidate?.FullName.AsLinuxPath();
	}

	private static bool IsCandidate(string aspNetCoreVersion, string dotNetRuntimeVersion)
	{
		var dotNetRuntimeVersionParts = dotNetRuntimeVersion.Split(".").AsNonNull();
		var aspNetCoreVersionParts = aspNetCoreVersion.Split(".").AsNonNull();
		for (var i = 0; i < dotNetRuntimeVersionParts.Length; i++)
		{
			if (i >= aspNetCoreVersionParts.Length) {
				continue;
			}
			if (aspNetCoreVersionParts[i] == dotNetRuntimeVersionParts[i]) {
				return true;
			}
		}
		return false;
	}
}
