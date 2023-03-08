using System;

namespace DryGen.MermaidFromDotnetDepsJson.Model
{
    internal abstract class BaseModelElement
    {
        protected BaseModelElement(string id, char delimiter = '/')
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }
            Id = id;
            var idParts = id.Split(delimiter);
            if (idParts.Length != 2)
            {
                throw $"Could not spilt '{id}' in name and version on delimiter '{delimiter}'".ToInvalidContentException();
            }
            Name = idParts[0];
            Version = idParts[1];
        }

        public string Id { get; }
        public string Name { get; }
        public string Version { get; }
    }
}
