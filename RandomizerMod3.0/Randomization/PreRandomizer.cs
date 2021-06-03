﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;
using static RandomizerMod.Randomization.Randomizer;

namespace RandomizerMod.Randomization
{
    internal static class PreRandomizer
    {
        public static void RandomizeNonShopCosts()
        {
            foreach (string item in LogicManager.ItemNames)
            {
                ReqDef def = LogicManager.GetItemDef(item);
                if (!RandomizerMod.Instance.Settings.GetRandomizeByPool(def.pool))
                {
                    RandomizerMod.Instance.Settings.AddNewCost(item, def.cost);
                    continue; //Skip cost rando if this item's pool is vanilla
                }

                if (def.costType == Actions.AddYNDialogueToShiny.CostType.Essence) //essence cost
                {
                    int cost = 1 + rand.Next(MAX_ESSENCE_COST);

                    def.cost = cost;
                    LogicManager.EditItemDef(item, def); // really shouldn't be editing this, bad idea
                    RandomizerMod.Instance.Settings.AddNewCost(item, cost);
                    continue;
                }

                if (def.costType == Actions.AddYNDialogueToShiny.CostType.Grub) //grub cost
                {
                    int cost = 1 + rand.Next(MAX_GRUB_COST);

                    def.cost = cost;
                    LogicManager.EditItemDef(item, def); // yeah, I'm probably not fixing it though
                    RandomizerMod.Instance.Settings.AddNewCost(item, cost);
                    continue;
                }
            }
        }

        public static void RandomizeStartingItems()
        {
            startItems = new List<string>();
            if (!RandomizerMod.Instance.Settings.RandomizeStartItems) return;

            List<string> pool1 = new List<string> { "Mantis_Claw", "Monarch_Wings" };
            List<string> pool2 = new List<string> { "Mantis_Claw", "Monarch_Wings", "Mothwing_Cloak", "Crystal_Heart" };
            List<string> pool3 = new List<string> { "Shade_Cloak", "Isma's_Tear", "Vengeful_Spirit", "Howling_Wraiths", "Desolate_Dive", "Cyclone_Slash", "Great_Slash", "Dash_Slash", "Dream_Nail" };
            List<string> pool4 = new List<string> { "City_Crest", "Lumafly_Lantern", "Tram_Pass", "Simple_Key-Sly", "Shopkeeper's_Key", "Elegant_Key", "Love_Key", "King's_Brand" };

            // If the player has split cloak, it's easiest to just remove the possibility they start with dash.
            if (RandomizerMod.Instance.Settings.RandomizeCloakPieces)
            {
                pool2.Remove("Mothwing_Cloak");
                pool3.Remove("Shade_Cloak");
            }

            startItems.Add(pool1[rand.Next(pool1.Count)]);

            pool2.Remove(startItems[0]);
            startItems.Add(pool2[rand.Next(pool2.Count)]);

            if (RandomizerMod.Instance.Settings.RandomizeClawPieces && startItems.Contains("Mantis_Claw"))
            {
                startItems.Remove("Mantis_Claw");
                startItems.Add("Left_Mantis_Claw");
                startItems.Add("Right_Mantis_Claw");
            }

            for (int i = rand.Next(4); i > 0; i--)
            {
                startItems.Add(pool3[rand.Next(pool3.Count)]);
                pool3.Remove(startItems.Last());
            }

            for (int i = rand.Next(7 - startItems.Count); i > 0; i--) // no more than 4 tier3 or tier4 items
            {
                startItems.Add(pool4[rand.Next(pool4.Count)]);
                pool4.Remove(startItems.Last());
            }

            for (int i = rand.Next(2) + 1; i > 0; i--)
            {
                List<string> charms = LogicManager.ItemNames.Where(_item => LogicManager.GetItemDef(_item).action == GiveItemActions.GiveAction.Charm).Except(startItems).ToList();
                startItems.Add(charms[rand.Next(charms.Count)]);
            }

            if (startProgression == null) startProgression = new List<string>();

            foreach (string item in startItems)
            {
                if (LogicManager.GetItemDef(item).progression) startProgression.Add(item);
            }
        }

        public static void RandomizeStartingLocation()
        {
            if (RandomizerMod.Instance.Settings.RandomizeStartLocation)
            {
                MiniPM pm = new MiniPM();
                pm.logicFlags["ITEMRANDO"] = !RandomizerMod.Instance.Settings.RandomizeTransitions;
                pm.logicFlags["AREARANDO"] = RandomizerMod.Instance.Settings.RandomizeAreas;
                pm.logicFlags["ROOMRANDO"] = RandomizerMod.Instance.Settings.RandomizeRooms;

                pm.logicFlags["MILDSKIPS"] = RandomizerMod.Instance.Settings.MildSkips;
                pm.logicFlags["SHADESKIPS"] = RandomizerMod.Instance.Settings.ShadeSkips;
                pm.logicFlags["ACIDSKIPS"] = RandomizerMod.Instance.Settings.AcidSkips;
                pm.logicFlags["FIREBALLSKIPS"] = RandomizerMod.Instance.Settings.FireballSkips;
                pm.logicFlags["SPIKETUNNELS"] = RandomizerMod.Instance.Settings.SpikeTunnels;
                pm.logicFlags["DARKROOMS"] = RandomizerMod.Instance.Settings.DarkRooms;
                pm.logicFlags["SPICYSKIPS"] = RandomizerMod.Instance.Settings.SpicySkips;

                pm.logicFlags["VERTICAL"] = RandomizerMod.Instance.Settings.RandomizeStartItems;

                List<string> startLocations = LogicManager.StartLocations
                    .Where(start => pm.Evaluate(LogicManager.GetStartLocation(start).logic))
                    .Except(new string[] { "King's Pass" })
                    .ToList();
                StartName = startLocations[rand.Next(startLocations.Count)];
            }
            else if (!LogicManager.StartLocations.Contains(RandomizerMod.Instance.Settings.StartName))
            {
                StartName = "King's Pass";
            }
            else StartName = RandomizerMod.Instance.Settings.StartName;

            Log("Setting start location as " + StartName);

            StartDef def = LogicManager.GetStartLocation(StartName);

            if (startProgression == null)
            {
                startProgression = new List<string>();
            }
            if (!RandomizerMod.Instance.Settings.RandomizeRooms)
            {
                startProgression.Add(def.waypoint);
            }
            if (RandomizerMod.Instance.Settings.RandomizeAreas && !string.IsNullOrEmpty(def.areaTransition))
            {
                startProgression.Add(def.areaTransition);
            }
            if (RandomizerMod.Instance.Settings.RandomizeRooms)
            {
                startProgression.Add(def.roomTransition);
            }
        }
    }
}
