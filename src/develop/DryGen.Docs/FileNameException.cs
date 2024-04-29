using System;

namespace DryGen.Docs;

public sealed class FileNameException : Exception
{
	public FileNameException(string fileName, string message) : base($"'{fileName}': {message}")
	{
	}
}