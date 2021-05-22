using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Randomization
{
    public static class SettingsExtensions
    {
        public static List<string> GetRandomizedItems(this GenerationSettings GS)
        {
            List<string> items = new List<string>();

            foreach (string pool in PoolSettings.FieldNames)
            {
                if (GS.PoolSettings.GetFieldByName(pool)) items.AddRange(Data.GetItemNamesByPool(pool));
            }

            if (GS.MiscSettings.AddDuplicateItems)
            {
                // TODO: Implement duplicate items
            }

            if (GS.CursedSettings.RandomizeFocus)
            {
                items.AddRange(Data.GetItemNamesByPool("Focus"));
            }

            if (GS.CursedSettings.SplitClaw)
            {
                items.RemoveAll(s => s == "Mantis_Claw");
                items.AddRange(Data.GetItemNamesByPool("SplitClaw"));
            }

            if (GS.CursedSettings.SplitCloak)
            {
                items.RemoveAll(s => s == "Mothwing_Cloak");
                items.AddRange(Data.GetItemNamesByPool("SplitCloak"));
            }
            
            if (GS.CursedSettings.RandomizeNail)
            {
                items.AddRange(Data.GetItemNamesByPool("CursedNail"));
            }

            return items;
        }

        public static List<string> GetRandomizedLocations(this GenerationSettings GS)
        {
            List<string> locations = new List<string>();

            locations.AddRange(Data.GetLocationNamesByPool("Shops"));

            foreach (string pool in PoolSettings.FieldNames)
            {
                if (GS.PoolSettings.GetFieldByName(pool)) locations.AddRange(Data.GetLocationNamesByPool(pool));
            }

            if (GS.CursedSettings.RandomizeFocus)
            {
                locations.AddRange(Data.GetLocationNamesByPool("Focus"));
            }

            if (GS.CursedSettings.SplitClaw)
            {
                locations.RemoveAll(s => s == "Mantis_Claw");
                locations.AddRange(Data.GetLocationNamesByPool("SplitClaw"));
            }

            if (GS.CursedSettings.SplitCloak)
            {
                locations.RemoveAll(s => s == "Mothwing_Cloak");
                locations.AddRange(Data.GetLocationNamesByPool("SplitCloak"));
            }

            if (GS.CursedSettings.RandomizeNail)
            {
                locations.AddRange(Data.GetLocationNamesByPool("CursedNail"));
            }

            return locations;
        }

    }
}
