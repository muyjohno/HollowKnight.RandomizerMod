using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class SkipSettings : ICloneable
    {
        public bool MildSkips;
        public bool ShadeSkips;
        public bool FireballSkips;
        public bool AcidSkips;
        public bool SpikeTunnels;
        public bool DarkRooms;
        public bool SpicySkips;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
