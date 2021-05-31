using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;
using RandomizerMod.RandomizerData;
using RandomizerMod.Randomization.Logic;
using RandomizerMod.Randomization.Util;

namespace RandomizerMod.Randomization
{
    public static class SettingsExtensions
    {
        public static List<string> GetRandomizedItems(this GenerationSettings GS)
        {
            List<string> items = new List<string>();
            
            foreach (string pool in Data.GetApplicablePools(GS))
            {
                items.AddRange(Data.GetItemNamesByPool(pool));
            }

            if (GS.MiscSettings.AddDuplicateItems)
            {
                // TODO: Implement duplicate items
            }

            if (GS.CursedSettings.SplitClaw)
            {
                items.RemoveAll(s => s == "Mantis_Claw");
            }

            if (GS.CursedSettings.SplitCloak)
            {
                items.RemoveAll(s => s == "Mothwing_Cloak");
            }

            return items;
        }

        public static List<string> GetRandomizedLocations(this GenerationSettings GS)
        {
            List<string> locations = new List<string>();

            locations.AddRange(Data.GetLocationNamesByPool("Shops"));

            foreach (string pool in Data.GetApplicablePools(GS))
            {
                locations.AddRange(Data.GetLocationNamesByPool(pool));
            }

            if (GS.CursedSettings.SplitClaw)
            {
                locations.RemoveAll(s => s == "Mantis_Claw");
            }

            if (GS.CursedSettings.SplitCloak)
            {
                locations.RemoveAll(s => s == "Mothwing_Cloak");
            }

            return locations;
        }

        public static List<string> GetRandomizedTransitions(this GenerationSettings GS)
        {
            switch (GS.TransitionSettings.Mode)
            {
                default:
                case TransitionSettings.TransitionMode.None:
                    return new List<string>();
                case TransitionSettings.TransitionMode.AreaRandomizer:
                    return Data.GetAreaTransitionNames().ToList();
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    return Data.GetRoomTransitionNames().ToList();
            }
        }

        public static List<ILP> GetVanillaProgression(this GenerationSettings GS, LogicManager LM)
        {
            return Data.GetApplicableVanillaDefs(GS).Where(def => LM.IsProgression(def.item)).Select(def => new ILP(def.item, def.location)).ToList();
        }

    }
}
