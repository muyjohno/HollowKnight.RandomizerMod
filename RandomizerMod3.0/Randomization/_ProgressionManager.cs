using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using static RandomizerMod.LogHelper;
using RandomizerMod.Settings;

namespace RandomizerMod.Randomization
{
    public class _ProgressionManager
    {
        public int[] obtained;
        private Dictionary<string, int> grubLocations;
        private Dictionary<string, int> essenceLocations;
        private Dictionary<string, int> flameLocations;
        private GenerationSettings GEN;
        private bool temp;
        private bool share = true;
        private bool concealRandom;
        private int randomEssence = 0;
        private int randomFlames = 0;
        public HashSet<string> tempItems;

        // TODO: Add GenerationSettings to constructor
        public _ProgressionManager(RandomizerState state, int[] progression = null)
        {
            concealRandom = state == RandomizerState.HelperLog;

            obtained = new int[_LogicManager.bitMaskMax + 1];
            if (progression != null) progression.CopyTo(obtained, 0);

            FetchEssenceLocations(state);
            FetchGrubLocations(state);
            FetchFlameLocations(state);

            ApplyDifficultySettings();
            RecalculateEssence();
            RecalculateGrubs();
            RecalculateFlames();
        }

        public bool CanGet(string item)
        {
            return _LogicManager.ParseProcessedLogic(item, obtained);
        }

        public void Add(string item)
        {
            item = _LogicManager.RemoveDuplicateSuffix(item);
            if (!_LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] |= a.Item1;
            if (temp)
            {
                tempItems.Add(item);
            }
            if (share)
            {
                Share(item);
            }

            // Take into account root essence found; this should only ever happen during helper log generation
            if (RandomizerMod.Instance.Settings.RandomizeWhisperingRoots && concealRandom)
            {
                if (_LogicManager.TryGetItemDef(item, out ReqDef itemDef))
                {
                    if (itemDef.pool == "Root")
                    {
                        randomEssence += itemDef.geo;
                    }
                }
            }
            if (RandomizerMod.Instance.Settings.RandomizeBossEssence && concealRandom)
            {
                if (_LogicManager.TryGetItemDef(item, out ReqDef itemDef))
                {
                    if (itemDef.pool == "Essence_Boss")
                    {
                        randomEssence += itemDef.geo;
                    }
                }
            }
            if (RandomizerMod.Instance.Settings.RandomizeGrimmkinFlames && concealRandom)
            {
                if (_LogicManager.TryGetItemDef(item, out ReqDef itemDef))
                {
                    if (itemDef.pool == "Flame")
                    {
                        randomFlames += 1;
                    }
                }
            }

            RecalculateGrubs();
            RecalculateEssence();
            RecalculateFlames();
            UpdateWaypoints();
        }

        public void Add(IEnumerable<string> items)
        {
            foreach (string item in items.Select(i => _LogicManager.RemoveDuplicateSuffix(i)))
            {
                if (!_LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
                {
                    RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                    return;
                }
                obtained[a.Item2] |= a.Item1;
                if (temp)
                {
                    tempItems.Add(item);
                }
                if (share)
                {
                    Share(item);
                }
            }
            RecalculateGrubs();
            RecalculateEssence();
            RecalculateFlames();
            UpdateWaypoints();
        }

        public void AddTemp(string item)
        {
            temp = true;
            if (tempItems == null)
            {
                tempItems = new HashSet<string>();
            }
            Add(item);
        }

        private void Share(string item)
        {
            if (ItemManager.recentProgression != null)
            {
                ItemManager.recentProgression.Add(item);
            }

            if (TransitionManager.recentProgression != null)
            {
                TransitionManager.recentProgression.Add(item);
            }
        }

        private void ToggleShare(bool value)
        {
            share = value;
        }

        public void Remove(string item)
        {
            item = _LogicManager.RemoveDuplicateSuffix(item);
            if (!_LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] &= ~a.Item1;
            if (_LogicManager.grubProgression.Contains(item)) RecalculateGrubs();
            if (_LogicManager.essenceProgression.Contains(item)) RecalculateEssence();
            if (_LogicManager.flameProgression.Contains(item)) RecalculateFlames();
        }

        public void RemoveTempItems()
        {
            temp = false;
            foreach (string item in tempItems)
            {
                Remove(item);
            }
            tempItems = new HashSet<string>();
        }

        public void SaveTempItems()
        {
            temp = false;
            if (share)
            {
                foreach (string item in tempItems)
                {
                    Share(item);
                }
            }
            
            tempItems = new HashSet<string>();
        }

        public bool Has(string item)
        {
            item = _LogicManager.RemoveDuplicateSuffix(item);
            if (!_LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return false;
            }
            return (obtained[a.Item2] & a.Item1) == a.Item1;
        }

        public void UpdateWaypoints()
        {
            if (RandomizerMod.Instance.Settings.RandomizeRooms) return;

            foreach(string waypoint in _LogicManager.Waypoints)
            {
                if (!Has(waypoint) && CanGet(waypoint))
                {
                    Add(waypoint);
                }
            }
        }

        private void ApplyDifficultySettings()
        {
            bool tempshare = share;
            share = false;

            SkipSettings SKIP = GEN.SkipSettings;
            CursedSettings CURSE = GEN.CursedSettings;

            if (SKIP.ShadeSkips) Add("SHADESKIPS");
            if (SKIP.AcidSkips) Add("ACIDSKIPS");
            if (SKIP.SpikeTunnels) Add("SPIKETUNNELS");
            if (SKIP.SpicySkips) Add("SPICYSKIPS");
            if (SKIP.FireballSkips) Add("FIREBALLSKIPS");
            if (SKIP.DarkRooms) Add("DARKROOMS");
            if (SKIP.MildSkips) Add("MILDSKIPS");
            if (CURSE.RemoveSpellUpgrades) Add("CURSED");
            if (!CURSE.RandomizeFocus) Add("NONRANDOMFOCUS");
            if (!CURSE.RandomizeNail) Add("NONRANDOMNAIL");

            share = tempshare;
        }

        private Dictionary<string, int> FetchLocationsByPool(RandomizerState state, string pool)
        {
            Dictionary<string, int> locations;
            switch (state)
            {
                case RandomizerState.InProgress:
                    return new Dictionary<string, int>();
                case RandomizerState.Validating:
                    locations = ItemManager.nonShopItems.Where(kvp => _LogicManager.GetItemDef(kvp.Value).pool == pool).ToDictionary(kvp => kvp.Value, kvp => 1);
                    foreach (var kvp in ItemManager.shopItems)
                    {
                        if (kvp.Value.Any(item => _LogicManager.GetItemDef(item).pool == pool))
                        {
                            locations.Add(kvp.Key, kvp.Value.Count(item => _LogicManager.GetItemDef(item).pool == pool));
                        }
                    }
                    return locations;
                case RandomizerState.Completed:
                case RandomizerState.HelperLog:
                    locations = RandomizerMod.Instance.Settings.ItemPlacements
                        .Where(pair => _LogicManager.GetItemDef(pair.Item1).pool == pool && !_LogicManager.ShopNames.Contains(pair.Item2))
                        .ToDictionary(pair => pair.Item2, kvp => 1);
                    foreach (string shop in _LogicManager.ShopNames)
                    {
                        if (RandomizerMod.Instance.Settings.ItemPlacements.Any(pair => pair.Item2 == shop && _LogicManager.GetItemDef(pair.Item1).pool == pool))
                        {
                            locations.Add(shop, RandomizerMod.Instance.Settings.ItemPlacements.Count(pair => pair.Item2 == shop && _LogicManager.GetItemDef(pair.Item1).pool == pool));
                        }
                    }
                    return locations;
                default:
                    Log("FetchLocationsByPool: unexpected RandomizerState");
                    return new Dictionary<string, int>();
            }
        }

        private void FetchGrubLocations(RandomizerState state)
        {
            if (RandomizerMod.Instance.Settings.RandomizeGrubs)
            {
                grubLocations = FetchLocationsByPool(state, "Grub");
            }
            else
            {
                grubLocations = _LogicManager.GetItemsByPool("Grub").ToDictionary(grub => grub, grub => 1);
            }
        }

        private void FetchEssenceLocationsFromPool(RandomizerState state, string pool, bool poolRandomized)
        {
            switch (state)
            {
                default:
                    foreach (string root in _LogicManager.GetItemsByPool(pool))
                    {
                        essenceLocations.Add(root, _LogicManager.GetItemDef(root).geo);
                    }
                    break;
                case RandomizerState.InProgress when poolRandomized:
                case RandomizerState.HelperLog when poolRandomized:
                    break;
                case RandomizerState.Validating when poolRandomized:
                    foreach (var kvp in ItemManager.nonShopItems)
                    {
                        if (_LogicManager.GetItemDef(kvp.Value).pool == pool)
                        {
                            essenceLocations.Add(kvp.Key, _LogicManager.GetItemDef(kvp.Value).geo);
                        }
                    }
                    foreach (var kvp in ItemManager.shopItems)
                    {
                        foreach (string item in kvp.Value)
                        {
                            if (_LogicManager.GetItemDef(item).pool == pool)
                            {
                                if (!essenceLocations.ContainsKey(kvp.Key))
                                {
                                    essenceLocations.Add(kvp.Key, 0);
                                }
                                essenceLocations[kvp.Key] += _LogicManager.GetItemDef(item).geo;
                            }
                        }
                    }
                    break;
                case RandomizerState.Completed when poolRandomized:
                    foreach (var pair in RandomizerMod.Instance.Settings.ItemPlacements)
                    {
                        if (_LogicManager.GetItemDef(pair.Item1).pool == pool && !_LogicManager.ShopNames.Contains(pair.Item2))
                        {
                            essenceLocations.Add(pair.Item2, _LogicManager.GetItemDef(pair.Item1).geo);
                        }
                    }
                    foreach (string shop in _LogicManager.ShopNames)
                    {
                        if (RandomizerMod.Instance.Settings.ItemPlacements.Any(pair => pair.Item2 == shop && _LogicManager.GetItemDef(pair.Item1).pool == pool))
                        {
                            essenceLocations.Add(shop, 0);
                            foreach (var pair in RandomizerMod.Instance.Settings.ItemPlacements)
                            {
                                if (pair.Item2 == shop && _LogicManager.GetItemDef(pair.Item1).pool == pool)
                                {
                                    essenceLocations[shop] += _LogicManager.GetItemDef(pair.Item1).geo;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void FetchEssenceLocations(RandomizerState state)
        {
            essenceLocations = new Dictionary<string, int>();
            FetchEssenceLocationsFromPool(state, "Root", RandomizerMod.Instance.Settings.RandomizeWhisperingRoots);
            FetchEssenceLocationsFromPool(state, "Essence_Boss", RandomizerMod.Instance.Settings.RandomizeBossEssence);
        }

        private void FetchFlameLocations(RandomizerState state)
        {
            if (state == RandomizerState.HelperLog || !RandomizerMod.Instance.Settings.RandomizeGrimmkinFlames)
            {
                // Flames are not relevant for logic when they're not randomized, as the player starts with 6 of them already given.
                // When in Helper Log mode, we do not want to add vanilla flame locations!
                flameLocations = new Dictionary<string, int>();
            }
            else
            {
                flameLocations = FetchLocationsByPool(state, "Flame");
            }
        }

        public void RecalculateEssence()
        {
            int essence = randomEssence;

            foreach (string location in essenceLocations.Keys)
            {
                if (CanGet(location))
                {
                    essence += essenceLocations[location];
                }
                if (essence >= _Randomizer.MAX_ESSENCE_COST + _LogicManager.essenceTolerance) break;
            }
            obtained[_LogicManager.essenceIndex] = essence;
        }

        public void RecalculateGrubs()
        {
            int grubs = 0;

            foreach (string location in grubLocations.Keys)
            {
                if (CanGet(location))
                {
                    grubs += grubLocations[location];
                }
                if (grubs >= _Randomizer.MAX_GRUB_COST + _LogicManager.grubTolerance) break;
            }

            obtained[_LogicManager.grubIndex] = grubs;
        }

        public void RecalculateFlames()
        {
            int flames = randomFlames;

            foreach (string location in flameLocations.Keys)
            {
                if (CanGet(location))
                {
                    flames += flameLocations[location];
                }
            }

            obtained[_LogicManager.flameIndex] = flames;
        }

        public void AddGrubLocation(string location)
        {
            if (!grubLocations.ContainsKey(location))
            {
                grubLocations.Add(location, 1);
            }
            else
            {
                grubLocations[location]++;
            }
        }

        public void AddEssenceLocation(string location, int essence)
        {
            if (!essenceLocations.ContainsKey(location))
            {
                essenceLocations.Add(location, essence);
            }
            else
            {
                essenceLocations[location] += essence;
            }
        }

        public void AddFlameLocation(string location)
        {
            if (!flameLocations.ContainsKey(location))
            {
                flameLocations.Add(location, 1);
            }
            else
            {
                flameLocations[location]++;
            }
        }

        // useful for debugging
        public string ListObtainedProgression()
        {
            string progression = string.Empty;
            foreach (string item in _LogicManager.ItemNames)
            {
                if (_LogicManager.GetItemDef(item).progression && Has(item)) progression += item + ", ";
            }

            if (RandomizerMod.Instance.Settings.RandomizeTransitions)
            {
                foreach (string transition in _LogicManager.TransitionNames())
                {
                    if (Has(transition)) progression += transition + ", ";
                }
            }
            
            return progression;
        }
        public void SpeedTest()
        {
            Stopwatch watch = new Stopwatch();
            foreach (string item in _LogicManager.ItemNames)
            {
                watch.Reset();
                watch.Start();
                string result = CanGet(item).ToString();
                double elapsed = watch.Elapsed.TotalSeconds;
                Log("Parsed logic for " + item + " with result " + result + " in " + watch.Elapsed.TotalSeconds);
            }
        }
    }
}
