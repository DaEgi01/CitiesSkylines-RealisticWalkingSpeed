using System;

namespace RealisticWalkingSpeed
{
    public class ModFullTitle
    {
        private readonly string _modName;
        private readonly string _modVersion;

        public ModFullTitle(string modName, string modVersion)
        {
            _modName = modName ?? throw new ArgumentNullException(nameof(modName));
            _modVersion = modVersion ?? throw new ArgumentNullException(nameof(modVersion));
        }

        public static implicit operator string(ModFullTitle modFullTitle)
        {
            return modFullTitle._modName + " v" + modFullTitle._modVersion;
        }
    }
}
