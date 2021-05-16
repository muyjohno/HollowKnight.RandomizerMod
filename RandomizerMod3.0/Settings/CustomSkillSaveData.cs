using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class CustomSkillSaveData : ICloneable
    {
        //TODO: Update items which use ModSettings.SetBool in GiveItem
        public bool canFocus;
        //public bool canSwim;
        // shhh

        public bool canDashLeft;
        public bool canDashRight;
        public bool hasDashAny => canDashLeft || canDashRight;

        public bool hasWalljumpLeft;
        public bool hasWalljumpRight;
        public bool hasWalljumpAny => hasWalljumpLeft || hasWalljumpRight;

        public bool canUpslash;
        public bool canSideslashLeft;
        public bool canSideslashRight;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
