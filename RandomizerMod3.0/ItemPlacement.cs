using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.RandomizerData;

namespace RandomizerMod
{
    public class ItemPlacement
    {
        public ItemDef Item;
        public LocationDef Location;
        public int ID;

        public bool Obtained() => Ref.PLACEMENTS.IsObtained(ID);
    }
}
