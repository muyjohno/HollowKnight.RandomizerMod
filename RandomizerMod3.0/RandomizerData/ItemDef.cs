using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Randomization;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.RandomizerData
{
    public class ItemDef
    {
        public string name;

        // Control variables
        public string fieldName;
        // fieldName

        public ItemType type;
        public GiveAction action;
        public string pool;

        // charm variables
        public int charmNum;
        public string equipBoolName;

        // Big item variables
        public string bigSpriteKey;
        public string takeKey;
        public string nameKey;
        public string buttonKey;
        public string descOneKey;
        public string descTwoKey;

        // Shop variables
        public string shopName;
        public int shopCost;
        public string shopBool;
        public string shopDescKey;
        public string shopSpriteKey;
        public string notchCost;

        // Item tier flags
        public bool progression;
        public bool itemCandidate; // progression items which may open new locations in a pinch
        public bool majorItem; // reserved for the most useful items in the randomizer

        // Geo flags
        public bool inChest;
        public int amount;

        // Lifeblood flags
        public int lifeblood;

        // Lore flags
        public string loreSheet;
        public string loreKey;
        public TextType textType;
        public string inspectName;
        public string inspectFsmName;

        public string chestName;
        public string chestFsmName;
    }

}
