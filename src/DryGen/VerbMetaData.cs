using System;
using System.Collections.Generic;

namespace DryGen
{
    public class VerbMetadata
    {
        private readonly Type optionsType;

        public VerbMetadata(Type optionsType)
        {
            this.optionsType = optionsType;
        }

        public string HelpText => optionsType.GetVerbHelpText();

        public IReadOnlyList<OptionMetadata> Options => optionsType.GetOptionMetadataList();
    }
}
