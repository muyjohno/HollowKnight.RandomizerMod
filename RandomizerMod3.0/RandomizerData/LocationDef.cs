using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class LocationDef
    {
        public string sceneName;
        public string objectName;
        public string altObjectName;
        public string fsmName;
        public bool replace;
        public string selfDestructFsmName;

        public string pool;
        public string areaName;

        // New Shiny fields
        public bool newShiny;
        public float x;
        public float y;

        // Real Object fields
        public float elevation;

        // Chest fields
        public bool inChest;
        public string chestName;
        public string chestFsmName;

        // Shop fields
        public bool shop;
        public bool dungDiscount;
        public string requiredPlayerDataBool;

        // Lore flags
        public string inspectName;
        public string inspectFsmName;

        // Cost fields
        public int cost;
        public CostType costType;
    }
}
