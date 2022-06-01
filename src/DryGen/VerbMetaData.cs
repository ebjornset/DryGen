using System;

namespace DryGen
{
    public class VerbMetaData
    {
        private readonly Type optionsType;

        public VerbMetaData(Type optionsType)
        {
            this.optionsType = optionsType;
        }

        public string Verb => optionsType.GetVerb();
        public string HelpText => optionsType.GetVerbHelpText();
    }
}
