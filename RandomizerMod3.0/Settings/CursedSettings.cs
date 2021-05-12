using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class CursedSettings : ICloneable
    {
        public bool RandomCurses;
        public bool RandomizeFocus;
        public bool RandomizeNail;
        public bool LongerProgressionChains;
        public bool ReplaceJunkWithOneGeo;
        public bool RemoveSpellUpgrades;
        public bool SplitClaw;
        public bool SplitCloak;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
