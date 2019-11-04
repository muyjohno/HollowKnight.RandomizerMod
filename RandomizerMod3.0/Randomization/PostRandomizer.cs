using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;
using static RandomizerMod.Randomization.Randomizer;

namespace RandomizerMod.Randomization
{
    internal static class PostRandomizer
    {
        public static void PostRandomizationTasks()
        {
            SaveAllPlacements();
            SaveItemHints();
            //No vanilla'd loctions in the spoiler log, please!
            (string, string)[] itemPairs = RandomizerMod.Instance.Settings.ItemPlacements.Except(VanillaManager.Instance.ItemPlacements).ToArray();
            if (RandomizerMod.Instance.Settings.CreateSpoilerLog) RandoLogger.LogAllToSpoiler(itemPairs, RandomizerMod.Instance.Settings._transitionPlacements.Select(kvp => (kvp.Key, kvp.Value)).ToArray());
        }

        private static void SaveAllPlacements()
        {
            if (RandomizerMod.Instance.Settings.RandomizeTransitions)
            {
                foreach (KeyValuePair<string, string> kvp in TransitionManager.transitionPlacements)
                {
                    RandomizerMod.Instance.Settings.AddTransitionPlacement(kvp.Key, kvp.Value);
                }
            }

            foreach (KeyValuePair<string, List<string>> kvp in ItemManager.shopItems)
            {
                foreach (string item in kvp.Value)
                {
                    RandomizeShopCost(item);
                }
            }

            foreach ((string, string) pair in GetPlacedItemPairs())
            {
                RandomizerMod.Instance.Settings.AddItemPlacement(pair.Item1, pair.Item2);
            }

            for (int i = 0; i < startItems.Count; i++)
            {
                RandomizerMod.Instance.Settings.AddItemPlacement(startItems[i], "Equipped_(" + i + ")");
            }

            RandomizerMod.Instance.Settings.StartName = StartName;

        }

        public static int RandomizeShopCost(string item)
        {
            rand = new Random(RandomizerMod.Instance.Settings.Seed + item.GetHashCode()); // make shop item cost independent from prior randomization

            // Give a shopCost to every shop item
            ReqDef def = LogicManager.GetItemDef(item);
            int priceFactor = 1;
            if (def.geo > 0) priceFactor = 0;
            if (item.StartsWith("Rancid") || item.StartsWith("Mask")) priceFactor = 2;
            if (item.StartsWith("Pale_Ore") || item.StartsWith("Charm_Notch")) priceFactor = 3;
            if (item.StartsWith("Godtuner") || item.StartsWith("Collector") || item.StartsWith("World_Sense")) priceFactor = 0;

            int cost;
            if (RandomizerMod.Instance.Settings.GetRandomizeByPool(def.pool))
            {
                cost = (100 + rand.Next(41) * 10) * priceFactor;
            }
            else
            {
                cost = def.shopCost;
            }
            cost = Math.Max(cost, 1);

            def.shopCost = cost;
            LogicManager.EditItemDef(item, def);
            RandomizerMod.Instance.Settings.AddShopCost(item, cost);

            return cost;
        }

        private static void SaveItemHints()
        {
            //Create a randomly ordered list of all major items in nonshop locations
            List<string> goodPools = new List<string> { "Dreamer", "Skill", "Charm", "Key" };
            List<string> possibleHintItems = ItemManager.nonShopItems.Values.Where(val => goodPools.Contains(LogicManager.GetItemDef(val).pool)).ToList();
            Dictionary<string, string> inverseNonShopItems = ItemManager.nonShopItems.ToDictionary(x => x.Value, x => x.Key); // There can't be two items at the same location, so this inversion is safe
            while (possibleHintItems.Count > 0)
            {
                string item = possibleHintItems[rand.Next(possibleHintItems.Count)];
                RandomizerMod.Instance.Settings.AddNewHint(item, inverseNonShopItems[item]);
                possibleHintItems.Remove(item);
            }
        }

        public static List<(string, string)> GetPlacedItemPairs()
        {
            List<(string, string)> pairs = new List<(string, string)>();
            foreach (KeyValuePair<string, List<string>> kvp in ItemManager.shopItems)
            {
                foreach (string item in kvp.Value)
                {
                    pairs.Add((item, kvp.Key));
                }
            }
            foreach (KeyValuePair<string, string> kvp in ItemManager.nonShopItems)
            {
                pairs.Add((kvp.Value, kvp.Key));
            }

            //Vanilla Item Placements (for RandomizerActions, Hints, Logs, etc)
            foreach ((string, string) pair in vm.ItemPlacements)
            {
                pairs.Add((pair.Item1, pair.Item2));
            }

            return pairs;
        }

        public static void LogItemPlacements(ProgressionManager pm)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("All Item Placements:");
            foreach ((string, string) pair in GetPlacedItemPairs())
            {
                ReqDef def = LogicManager.GetItemDef(pair.Item1);
                if (def.progression) sb.AppendLine($"--{pm.CanGet(pair.Item2)} - {pair.Item1} -at- {pair.Item2}");
            }

            Log(sb.ToString());
        }
    }
}
