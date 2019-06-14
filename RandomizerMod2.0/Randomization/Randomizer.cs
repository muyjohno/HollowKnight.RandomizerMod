using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using RandomizerMod.Actions;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Randomization
{
    internal static class Randomizer
    {
        private static Dictionary<string, List<string>> shopItems;
        public static Dictionary<string, string> nonShopItems;

        private static List<string> unobtainedLocations;
        private static List<string> unobtainedItems;
        private static long obtainedProgression;
        private static List<string> storedItems; //Nonrandomized progression items. Randomizer checks if any new storedItems are accessible on each round
        public static List<string> randomizedItems; //Non-geo, non-shop randomized items. Mainly used as a candidates list for the hint shop.
        private static List<string> geoItems;
        private static List<string> shopNames;
        private static List<string> reachableShops;
        private static List<string> junkStandby;
        private static List<string> progressionStandby;
        private static List<string> locationStandby;
        private static long settingsList;
        private static List<string> reachableLocations;

        private static int randomizerAttempts;
        private static int shopMax;

        private static bool overflow;
        private static bool initialized;
        private static bool randomized;
        private static bool validated;
        public static bool Done { get; private set; }

        public static void Randomize()
        {
            RandomizerMod.Instance.Settings.ResetItemPlacements();
            Log("Randomizing with seed: " + RandomizerMod.Instance.Settings.Seed);
            Log("Mode - " + (RandomizerMod.Instance.Settings.NoClaw ? "No Claw" : "Standard"));
            Log("Shade skips - " + RandomizerMod.Instance.Settings.ShadeSkips);
            Log("Acid skips - " + RandomizerMod.Instance.Settings.AcidSkips);
            Log("Spike tunnel skips - " + RandomizerMod.Instance.Settings.SpikeTunnels);
            Log("Misc skips - " + RandomizerMod.Instance.Settings.MiscSkips);
            Log("Fireball skips - " + RandomizerMod.Instance.Settings.FireballSkips);
            Log("Mag skips - " + RandomizerMod.Instance.Settings.MagSkips);

            Random rand = new Random(RandomizerMod.Instance.Settings.Seed);

            Stopwatch randomizerWatch = new Stopwatch();
            Stopwatch validationWatch = new Stopwatch();

            initialized = false;
            randomizerAttempts = 0;

            while (true)
            {
                if (!initialized)
                {
                    randomizerWatch.Start();
                    SetupVariables();
                    randomizerAttempts++;
                    reachableLocations = new List<string>();
                    foreach (string location in unobtainedLocations)
                    {
                        if (!reachableLocations.Contains(location) && LogicManager.ParseProcessedLogic(location, obtainedProgression)) reachableLocations.Add(location);
                    }
                    initialized = true;
                    RandomizerMod.Instance.Log("Beginning first pass...");

                }

                else if (!randomized)
                {
                    string placeItem = string.Empty;
                    string placeLocation = string.Empty;
                    List<string> progressionItems = new List<string>();
                    List<string> candidateItems = new List<string>();
                    int reachableCount = reachableLocations.Count;

                    // Check for progression items from a nonrandomized category
                    foreach (string itemName in storedItems)
                    {
                        if ((LogicManager.progressionBitMask[itemName] & obtainedProgression) != LogicManager.progressionBitMask[itemName] &&  LogicManager.ParseProcessedLogic(itemName, obtainedProgression))
                        {
                            obtainedProgression |= LogicManager.progressionBitMask[itemName];
                        }
                    }

                    // Acquire unweighted accessible locations
                    foreach (string location in unobtainedLocations)
                    {
                        if (!reachableLocations.Contains(location) && LogicManager.ParseProcessedLogic(location, obtainedProgression)) reachableLocations.Add(location);
                    }
                    reachableCount = reachableLocations.Count;

                    // First, we place all geo items, to avoid them ending up in shops
                    if (geoItems.Count > 0)
                    {
                        // Traditional early geo pickup
                        if (RandomizerMod.Instance.Settings.EarlyGeo && RandomizerMod.Instance.Settings.RandomizeCharms && unobtainedLocations.Contains("Fury_of_the_Fallen"))
                        {
                            string[] furyGeoContenders = geoItems.Where(item => LogicManager.GetItemDef(item).geo > 100).ToArray();
                            string furyGeoItem = furyGeoContenders[rand.Next(furyGeoContenders.Length)];

                            unobtainedItems.Remove(furyGeoItem);
                            unobtainedLocations.Remove("Fury_of_the_Fallen");
                            nonShopItems.Add("Fury_of_the_Fallen", furyGeoItem);
                            geoItems.Remove(furyGeoItem);
                            continue;
                        }
                        // If charms aren't randomized, then early geo is here
                        else if (RandomizerMod.Instance.Settings.EarlyGeo && !RandomizerMod.Instance.Settings.RandomizeCharms && unobtainedLocations.Contains("False_Knight_Chest"))
                        {
                            string[] furyGeoContenders = geoItems.Where(item => LogicManager.GetItemDef(item).geo > 100).ToArray();
                            string furyGeoItem = furyGeoContenders[rand.Next(furyGeoContenders.Length)];

                            unobtainedItems.Remove(furyGeoItem);
                            unobtainedLocations.Remove("False_Knight_Chest");
                            reachableLocations.Remove("False_Knight_Chest");
                            nonShopItems.Add("False_Knight_Chest", furyGeoItem);
                            geoItems.Remove(furyGeoItem);
                            continue;
                        }

                        else
                        {
                            string geoItem = geoItems[rand.Next(geoItems.Count)];
                            List<string> geoCandidates = unobtainedLocations.Except(reachableLocations).ToList(); // Pick geo locations which aren't in sphere 0, since fury is there
                            geoCandidates = geoCandidates.Where(location => !LogicManager.ShopNames.Contains(location) && LogicManager.GetItemDef(location).cost == 0).ToList(); // Another precaution - no geo pickups placed in shops or at toll items
                            string geoLocation = geoCandidates[rand.Next(geoCandidates.Count)];
                            unobtainedItems.Remove(geoItem);
                            unobtainedLocations.Remove(geoLocation);
                            nonShopItems.Add(geoLocation, geoItem);
                            geoItems.Remove(geoItem);
                            continue;
                        }
                    }

                    //Then, we place items randomly while there are many reachable spaces
                    else if (reachableCount > 1 && unobtainedItems.Count > 0)
                    {
                        placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                        placeLocation = reachableLocations[rand.Next(reachableLocations.Count)];
                    }
                    // This path handles forcing progression items when few random locations are left
                    else if (reachableCount == 1)
                    {
                        progressionItems = GetProgressionItems(); // Progression items which open new locations
                        candidateItems = GetCandidateItems(); // Filtered list of progression items which have compound item logic
                        if (progressionItems.Count > 0)
                        {
                            placeItem = progressionItems[rand.Next(progressionItems.Count)];
                            placeLocation = reachableLocations[0];
                        }
                        else if (unobtainedLocations.Count > 1 && candidateItems.Count > 0)
                        {
                            overflow = true;
                            placeItem = candidateItems[rand.Next(candidateItems.Count)];
                            progressionStandby.Add(placeItem); // Note that we don't have enough locations to place candidate items here, so they go onto a standby list until the second pass
                            unobtainedItems.Remove(placeItem);
                            obtainedProgression |= LogicManager.progressionBitMask[placeItem];
                            continue;
                        }
                        else // This is how the last reachable location is filled
                        {
                            placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                            placeLocation = reachableLocations[0];
                        }
                    }
                    else // No reachable locations, ready to proceed to next stage
                    {
                        randomized = true;
                        overflow = true;
                        continue;
                    }


                    // Until first overflow items are forced, we keep junk locations for later reshuffling
                    if (!overflow && !LogicManager.GetItemDef(placeItem).progression)
                    {
                        junkStandby.Add(placeItem);
                        locationStandby.Add(placeLocation);
                        reachableLocations.Remove(placeLocation);
                        unobtainedLocations.Remove(placeLocation);
                        unobtainedItems.Remove(placeItem);
                    }
                    else
                    {
                        reachableLocations.Remove(placeLocation);
                        unobtainedLocations.Remove(placeLocation);
                        unobtainedItems.Remove(placeItem);
                        if (LogicManager.GetItemDef(placeItem).progression)
                        {
                            obtainedProgression |= LogicManager.progressionBitMask[placeItem];
                            foreach (string location in unobtainedLocations)
                            {
                                if (!reachableLocations.Contains(location) && LogicManager.ParseProcessedLogic(location, obtainedProgression))
                                {
                                    reachableLocations.Add(location);
                                }
                            }
                        }

                        if (placeItem == "Shopkeeper's_Key" && !overflow) reachableShops.Add("Sly_(Key)"); //Reachable shops are those where we can place required items in the second pass. Important because Shopkey will not be forced as progression if shopMax < 5

                        if (shopItems.ContainsKey(placeLocation))
                        {
                            shopItems[placeLocation].Add(placeItem);
                        }
                        else
                        {
                            nonShopItems.Add(placeLocation, placeItem);
                        }
                        continue;
                    }
                }

                else if (overflow)
                {
                    foreach (string placeItem in junkStandby) unobtainedItems.Add(placeItem);
                    RandomizerMod.Instance.Log("First pass randomization complete.");
                    RandomizerMod.Instance.Log("Unused locations: " + unobtainedLocations.Count);
                    RandomizerMod.Instance.Log("Unused items: " + unobtainedItems.Count);
                    RandomizerMod.Instance.Log("Remaining required items: " + progressionStandby.Count);
                    RandomizerMod.Instance.Log("Reserved locations: " + locationStandby.Count);
                    RandomizerMod.Instance.Log("Beginning second pass...");

                    // First, we have to guarantee that items used in the logic chain are accessible
                    foreach (string placeItem in progressionStandby)
                    {
                        if (locationStandby.Count > 0)
                        {
                            string placeLocation = locationStandby[rand.Next(locationStandby.Count)];
                            locationStandby.Remove(placeLocation);
                            if (shopItems.ContainsKey(placeLocation))
                            {
                                shopItems[placeLocation].Add(placeItem);
                            }
                            else
                            {
                                nonShopItems.Add(placeLocation, placeItem);
                            }
                        }
                        else
                        {
                            string placeLocation = reachableShops[rand.Next(reachableShops.Count)];
                            shopItems[placeLocation].Add(placeItem);
                        }
                    }
                    
                    // We fill the remaining locations and shops with the leftover junk
                    while(unobtainedItems.Count > 0)
                    {
                        string placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                        unobtainedItems.Remove(placeItem);
                        if (unobtainedLocations.Count > 0)
                        {
                            string placeLocation = unobtainedLocations[rand.Next(unobtainedLocations.Count)];
                            unobtainedLocations.Remove(placeLocation);
                            if (shopItems.ContainsKey(placeLocation))
                            {
                                shopItems[placeLocation].Add(placeItem);
                            }
                            else
                            {
                                nonShopItems.Add(placeLocation, placeItem);
                            }
                        }
                        else if (locationStandby.Count > 0)
                        {
                            string placeLocation = locationStandby[rand.Next(locationStandby.Count)];
                            locationStandby.Remove(placeLocation);
                            if (shopItems.ContainsKey(placeLocation))
                            {
                                shopItems[placeLocation].Add(placeItem);
                            }
                            else
                            {
                                nonShopItems.Add(placeLocation, placeItem);
                            }
                        }
                        else
                        {
                            string placeLocation = shopNames[rand.Next(5)];
                            shopItems[placeLocation].Add(placeItem);
                        }
                    }
                    randomizerWatch.Stop();
                    RandomizerMod.Instance.Log("Seed generation completed in " + randomizerWatch.Elapsed.TotalSeconds + " seconds.");
                    randomizerWatch.Reset();
                    overflow = false;
                    
                }

                else if (!validated)
                {
                    validationWatch.Start();
                    RandomizerMod.Instance.Log("Beginning seed validation...");
                    List<string> floorItems = nonShopItems.Keys.ToList();
                    List<string> currentItemKeys = new List<string>();
                    List<string> currentItemValues = new List<string>();
                    long obtained = settingsList;
                    int passes = 0;
                    while (randomizedItems.Except(currentItemValues).Any())
                    {
                        foreach (string itemName in floorItems)
                        {
                            if (!currentItemKeys.Contains(itemName) && LogicManager.ParseProcessedLogic(itemName, obtained))
                            {
                                currentItemKeys.Add(itemName);
                                currentItemValues.Add(nonShopItems[itemName]);
                                if (LogicManager.GetItemDef(nonShopItems[itemName]).progression) obtained |= LogicManager.progressionBitMask[nonShopItems[itemName]];
                            }
                        }
                        foreach (string shopName in shopNames)
                        {
                            if (!currentItemKeys.Contains(shopName) && LogicManager.ParseProcessedLogic(shopName, obtained))
                            {
                                currentItemKeys.Add(shopName);
                                foreach (string newItem in shopItems[shopName])
                                {
                                    currentItemValues.Add(newItem);
                                    if (LogicManager.GetItemDef(newItem).progression) obtained |= LogicManager.progressionBitMask[newItem];
                                }
                            }
                        }
                        foreach (string itemName in storedItems)
                        {
                            if ((LogicManager.progressionBitMask[itemName] & obtained) != LogicManager.progressionBitMask[itemName] && LogicManager.ParseProcessedLogic(itemName, obtained))
                            {
                                obtained |= LogicManager.progressionBitMask[itemName];
                            }
                        }
                        passes++;
                        if (passes > 100) break;
                    }
                    if (passes > 100)
                    {
                        validationWatch.Stop();
                        validationWatch.Reset();
                        RandomizerMod.Instance.Log("Failed to validate! Attempting new randomization...");
                        initialized = false;
                        continue;
                    }
                    validationWatch.Stop();
                    RandomizerMod.Instance.Log("Seed validation completed in " + validationWatch.Elapsed.TotalSeconds + " seconds.");
                    validationWatch.Reset();
                    validated = true;
                }
                else break;
            }

            RandomizerMod.Instance.Log("Finished randomization with " + randomizerAttempts + " attempt(s).");
            LogAllPlacements();

            //Create a randomly ordered list of all "real" items in floor locations
            List<string> goodPools = new List<string> { "Dreamer", "Skill", "Charm", "Key" };
            List<string> possibleHintItems = nonShopItems.Values.Where(val => goodPools.Contains(LogicManager.GetItemDef(val).pool)).ToList();
            Dictionary<string, string> inverseNonShopItems = nonShopItems.ToDictionary(x => x.Value, x=> x.Key); // There can't be two items at the same location, so this inversion is safe
            while (possibleHintItems.Count > 0)
            {
                string item = possibleHintItems[rand.Next(possibleHintItems.Count)];
                RandomizerMod.Instance.Settings.AddNewHint(item, inverseNonShopItems[item]);
                possibleHintItems.Remove(item);
            }

            Done = true;
            RandomizerMod.Instance.Log("Randomization done");
        }



        private static void SetupVariables()
        {
            nonShopItems = new Dictionary<string, string>();
            shopItems = new Dictionary<string, List<string>>();
            foreach (string shopName in LogicManager.ShopNames)
            {
                shopItems.Add(shopName, new List<string>());
            }

            unobtainedLocations = new List<string>();
            foreach (string itemName in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(itemName).type != ItemType.Shop)
                {
                    unobtainedLocations.Add(itemName);
                }
            }

            unobtainedLocations.AddRange(shopItems.Keys);
            unobtainedItems = LogicManager.ItemNames.ToList();
            shopNames = LogicManager.ShopNames.ToList();
            storedItems = new List<string>();
            randomizedItems = new List<string>();
            junkStandby = new List<string>();
            progressionStandby = new List<string>();
            locationStandby = new List<string>();

            //set up difficulty settings
            settingsList = 0;
            if (RandomizerMod.Instance.Settings.ShadeSkips) settingsList |= (LogicManager.progressionBitMask["SHADESKIPS"]);
            if (RandomizerMod.Instance.Settings.AcidSkips) settingsList |= (LogicManager.progressionBitMask["ACIDSKIPS"]);
            if (RandomizerMod.Instance.Settings.SpikeTunnels) settingsList |= (LogicManager.progressionBitMask["SPIKETUNNELS"]);
            if (RandomizerMod.Instance.Settings.MiscSkips) settingsList |= (LogicManager.progressionBitMask["MISCSKIPS"]);
            if (RandomizerMod.Instance.Settings.FireballSkips) settingsList |= (LogicManager.progressionBitMask["FIREBALLSKIPS"]);
            if (RandomizerMod.Instance.Settings.MagSkips) settingsList |= (LogicManager.progressionBitMask["MAGSKIPS"]);
            if (RandomizerMod.Instance.Settings.NoClaw) settingsList |= (LogicManager.progressionBitMask["NOCLAW"]);
            obtainedProgression = settingsList;



            // Don't place claw in no claw mode, obviously
            if (RandomizerMod.Instance.Settings.NoClaw)
            {
                unobtainedItems.Remove("Mantis_Claw");
            }

            foreach (string _itemName in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(_itemName).isFake)
                {
                    unobtainedLocations.Remove(_itemName);
                    unobtainedItems.Remove(_itemName);
                }
            }

            RemoveNonrandomizedItems();


            randomizedItems = unobtainedLocations.Except(LogicManager.ShopNames).ToList();
            Random rand = new Random(RandomizerMod.Instance.Settings.Seed);
            int eggCount = 1;
            foreach (string location in randomizedItems)
            {
                if (LogicManager.GetItemDef(location).longItemTier > RandomizerMod.Instance.Settings.LongItemTier)
                {
                    unobtainedLocations.Remove(location);
                    nonShopItems.Add(location, "Bonus_Arcane_Egg_(" + eggCount + ")");
                    eggCount++;
                }
            }

            if (RandomizerMod.Instance.Settings.PleasureHouse) nonShopItems.Add("Pleasure_House", "Small_Reward_Geo");

            geoItems = unobtainedItems.Where(name => LogicManager.GetItemDef(name).type == ItemType.Geo).ToList();
            randomizedItems = unobtainedLocations.Where(name => !LogicManager.ShopNames.Contains(name) && LogicManager.GetItemDef(name).type != ItemType.Geo).ToList();

            shopMax = unobtainedItems.Count - unobtainedLocations.Count + 5;

            if (shopMax < 5)
            {
                foreach (string shopName in LogicManager.ShopNames) unobtainedLocations.Remove(shopName);
            }
            reachableShops = LogicManager.ShopNames.ToList();
            reachableShops.Remove("Sly_(Key)");

            randomized = false;
            overflow = false;
            validated = false;
            Done = false;
        }

        private static void RemoveNonrandomizedItems()
        {
            if (!RandomizerMod.Instance.Settings.RandomizeDreamers)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Dreamer")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                        if (item.progression) storedItems.Add(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Dreamers left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeSkills)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Skill")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                        if (item.progression) storedItems.Add(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Skills left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeCharms)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Charm")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                        if (item.progression) storedItems.Add(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Charms left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeKeys)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Key")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                        if (item.progression) storedItems.Add(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Keys left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeGeoChests)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Geo")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Geo Chests left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeMaskShards)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Mask")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Mask Shards left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeVesselFragments)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Vessel")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Vessel Fragments left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeCharmNotches)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Notch")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Charm Notches left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizePaleOre)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Ore")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Pale Ore left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeRancidEggs)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Egg")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Rancid Eggs left in vanilla locations.");
            }
            if (!RandomizerMod.Instance.Settings.RandomizeRelics)
            {
                foreach (string _itemName in LogicManager.ItemNames)
                {
                    ReqDef item = LogicManager.GetItemDef(_itemName);
                    if (item.pool == "Relic")
                    {
                        unobtainedItems.Remove(_itemName);
                        unobtainedLocations.Remove(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Relics left in vanilla locations.");
            }
        }

        private static List<string> GetProgressionItems()
        {
            List<string> progression = new List<string>();
            unobtainedLocations.Remove(reachableLocations[0]);
            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).progression)
                {
                    long tempItem = LogicManager.progressionBitMask[str];
                    obtainedProgression |= tempItem;
                    foreach (string item in unobtainedLocations)
                    {
                        if (LogicManager.ParseProcessedLogic(item, obtainedProgression))
                        {
                            progression.Add(str);
                            break;
                        }
                    }
                    obtainedProgression &= ~tempItem;
                }
            }
            unobtainedLocations.Add(reachableLocations[0]);

            return progression;
        }

        private static List<string> GetCandidateItems()
        {
            List<string> progression = new List<string>();

            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).progression)
                {
                    // Baldur kills and Sprintmaster/Dashmaster/Sharpshadow are never good candidates, so we don't add them
                    if (str == "Mark_of_Pride" || str == "Longnail" || str == "Spore_Shroom" || str == "Glowing_Womb" || str == "Grubberfly's_Elegy" || str == "Weaversong" || str == "Sprintmaster" || str == "Dashmaster" || str == "Sharp_Shadow") { }
                    // Remove redundant items
                    else if (str == "Shopkeeper's_Key" || str == "Void_Heart" || str == "Shade_Soul" || str == "Abyss_Shriek" || str == "Descending_Dark" || str == "Dream_Gate") { }
                    //Place remainder
                    else
                    {
                        progression.Add(str);
                    }
                }
            }

            return progression;
        }

        private static void LogAllPlacements()
        {
            Log("Logging progression item placements:");
            foreach (KeyValuePair<string, List<string>> kvp in shopItems)
            {
                foreach (string item in kvp.Value)
                {
                    if (LogicManager.GetItemDef(item).progression) LogItemPlacement(item, kvp.Key);
                }
            }

            foreach (KeyValuePair<string, string> kvp in nonShopItems)
            {
                if (LogicManager.GetItemDef(kvp.Value).progression) LogItemPlacement(kvp.Value, kvp.Key);
            }

            Log("Logging ordinary item placements:");
            foreach (KeyValuePair<string, List<string>> kvp in shopItems)
            {
                foreach (string item in kvp.Value)
                {
                    if (!LogicManager.GetItemDef(item).progression) LogItemPlacement(item, kvp.Key);
                }
            }

            foreach (KeyValuePair<string, string> kvp in nonShopItems)
            {
                if (!LogicManager.GetItemDef(kvp.Value).progression) LogItemPlacement(kvp.Value, kvp.Key);
            }
        }

        private static void LogItemPlacement(string item, string location)
        {
            RandomizerMod.Instance.Settings.AddItemPlacement(item, location);
            Log(
                $"Putting item \"{item.Replace('_', ' ')}\" at \"{location.Replace('_', ' ')}\"");
        }
    }
}