using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using RandomizerMod.Actions;

using Random = System.Random;

namespace RandomizerMod.Randomization
{
    internal static class Randomizer
    {
        private static Dictionary<string, int> additiveCounts;

        private static Dictionary<string, List<string>> shopItems;
        public static Dictionary<string, string> nonShopItems;

        private static List<string> unobtainedLocations;
        private static List<string> unobtainedItems;
        private static List<string> obtainedItems;
        private static List<string> storedItems; //Nonrandomized progression items. Randomizer checks if any new storedItems are accessible on each round
        public static List<string> randomizedItems; //Non-geo, non-shop randomized items. Mainly used as a candidates list for the hint shop.
        private static List<string> geoItems;
        private static List<string> reachableShops;
        private static List<string> junkStandby;
        private static List<string> progressionStandby;
        private static List<string> locationStandby;

        private static int randomizerAttempts;
        private static int shopMax;

        private static List<RandomizerAction> actions;

        private static bool overflow;
        private static bool initialized;
        private static bool randomized;
        private static bool validated;
        public static bool Done { get; private set; }

        public static RandomizerAction[] Actions => actions.ToArray();

        public static void Randomize()
        {
            SetupVariables();

            RandomizerMod.Instance.Log("Randomizing with seed: " + RandomizerMod.Instance.Settings.Seed);
            RandomizerMod.Instance.Log("Mode - " + (RandomizerMod.Instance.Settings.NoClaw ? "No Claw" : "Standard"));
            RandomizerMod.Instance.Log("Shade skips - " + RandomizerMod.Instance.Settings.ShadeSkips);
            RandomizerMod.Instance.Log("Acid skips - " + RandomizerMod.Instance.Settings.AcidSkips);
            RandomizerMod.Instance.Log("Spike tunnel skips - " + RandomizerMod.Instance.Settings.SpikeTunnels);
            RandomizerMod.Instance.Log("Misc skips - " + RandomizerMod.Instance.Settings.MiscSkips);
            RandomizerMod.Instance.Log("Fireball skips - " + RandomizerMod.Instance.Settings.FireballSkips);
            RandomizerMod.Instance.Log("Mag skips - " + RandomizerMod.Instance.Settings.MagSkips);

            Random rand = new Random(RandomizerMod.Instance.Settings.Seed);

            #region General item randomizer

            Stopwatch randomizerWatch = new Stopwatch();
            Stopwatch validationWatch = new Stopwatch();

            while (true)
            {
                if (!initialized)
                {
                    randomizerWatch.Start();
                    SetupVariables();
                    randomizerAttempts++;
                    initialized = true;
                    RandomizerMod.Instance.Log("Beginning first pass...");
                }

                else if (!randomized)
                {
                    // Get currently reachable locations
                    List<string> reachableLocations = new List<string>();
                    string[] obtained = obtainedItems.ToArray();
                    string[] stored = storedItems.ToArray();
                    string placeItem = string.Empty;
                    string placeLocation = string.Empty;
                    List<string> progressionItems = new List<string>();
                    List<string> candidateItems = new List<string>();
                    int reachableCount = 0;

                    // Check for progression items from a nonrandomized category
                    foreach (string itemName in stored)
                    {
                        if (LogicManager.ParseLogic(itemName, obtained))
                        {
                            obtainedItems.Add(itemName);
                            obtained = obtainedItems.ToArray();
                            storedItems.Remove(itemName);
                            RandomizerMod.Instance.Log("Can now reach " + itemName.Replace('_', ' '));
                        }
                    }

                    // Acquire unweighted accessible locations
                    for (int i = 0; i < unobtainedLocations.Count; i++)
                    {
                        if (LogicManager.ParseLogic(unobtainedLocations[i], obtained))
                        {
                            reachableLocations.Add(unobtainedLocations[i]);
                            reachableCount++;
                        }
                    }

                    if (geoItems.Count > 0)
                    {
                        // Traditional early geo pickup
                        if (RandomizerMod.Instance.Settings.RandomizeCharms && unobtainedLocations.Contains("Fury_of_the_Fallen"))
                        {
                            string[] furyGeoContenders = geoItems.Where(item => LogicManager.GetItemDef(item).geo > 100).ToArray();
                            string furyGeoItem = furyGeoContenders[rand.Next(furyGeoContenders.Length)];

                            unobtainedItems.Remove(furyGeoItem);
                            unobtainedLocations.Remove("Fury_of_the_Fallen");
                            nonShopItems.Add("Fury_of_the_Fallen", furyGeoItem);
                            geoItems.Remove(furyGeoItem);
                            continue;
                        }
                        // If charms aren't randomized, then we always have vanilla or random geo at FK chest
                        else if (!RandomizerMod.Instance.Settings.RandomizeCharms && unobtainedLocations.Contains("False_Knight_Chest"))
                        {
                            string[] furyGeoContenders = geoItems.Where(item => LogicManager.GetItemDef(item).geo > 100).ToArray();
                            string furyGeoItem = furyGeoContenders[rand.Next(furyGeoContenders.Length)];

                            unobtainedItems.Remove(furyGeoItem);
                            unobtainedLocations.Remove("False_Knight_Chest");
                            nonShopItems.Add("False_Knight_Chest", furyGeoItem);
                            geoItems.Remove(furyGeoItem);
                            continue;
                        }

                        else
                        {
                            string geoItem = geoItems[rand.Next(geoItems.Count)];
                            List<string> geoCandidates = unobtainedLocations.Except(reachableLocations).ToList(); // Pick geo locations which aren't in sphere 0, since fury is there
                            geoCandidates = geoCandidates.Except(LogicManager.ShopNames).ToList(); // Another precaution - no geo pickups placed in shops
                            string geoLocation = geoCandidates[rand.Next(geoCandidates.Count)];
                            unobtainedItems.Remove(geoItem);
                            unobtainedLocations.Remove(geoLocation);
                            nonShopItems.Add(geoLocation, geoItem);
                            geoItems.Remove(geoItem);
                            continue;
                        }
                    }
                    else if (reachableCount > 1 && unobtainedItems.Count > 0)
                    {
                        placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                        placeLocation = reachableLocations[rand.Next(reachableLocations.Count)];
                    }
                    else if (unobtainedItems.Count == 0)
                    {
                        randomized = true;
                        overflow = true;
                        continue;
                    }
                    else if (reachableCount == 1)
                    {
                        progressionItems = GetProgressionItems(reachableCount); // Progression items which open new locations
                        candidateItems = GetCandidateItems(); // Filtered list of progression items which have compound item logic
                        if (progressionItems.Count > 0)
                        {
                            placeItem = progressionItems[rand.Next(progressionItems.Count)];
                            placeLocation = reachableLocations[0];
                            if (LogicManager.GetItemDef(placeItem).isGoodItem) placeItem = progressionItems[rand.Next(progressionItems.Count)]; // Something like Claw/Wings gets an extra reroll to incentivize more complex randomizations
                        }
                        else if (unobtainedLocations.Count > 1 && candidateItems.Count > 0)
                        {
                            overflow = true;
                            placeItem = candidateItems[rand.Next(candidateItems.Count)];
                            progressionStandby.Add(placeItem);
                            unobtainedItems.Remove(placeItem);
                            obtainedItems.Add(placeItem);
                            continue;
                        }
                        else
                        {
                            placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                            placeLocation = reachableLocations[0];
                        }
                    }
                    else
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
                        unobtainedLocations.Remove(placeLocation);
                        unobtainedItems.Remove(placeItem);
                    }
                    else
                    {
                        unobtainedLocations.Remove(placeLocation);
                        unobtainedItems.Remove(placeItem);
                        if (LogicManager.GetItemDef(placeItem).progression) obtainedItems.Add(placeItem);

                        if (placeItem == "Shopkeeper's_Key" && !overflow)
                        {
                            reachableShops.Add("Sly_(Key)");
                            if (shopMax > 5) unobtainedLocations.Add("Sly_(Key)");
                        }
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
                    foreach (string placeItem in unobtainedItems)
                    {
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
                            string placeLocation = reachableShops[rand.Next(reachableShops.Count)];
                            shopItems[placeLocation].Add(placeItem);
                        }
                    }
                    randomizerWatch.Stop();
                    RandomizerMod.Instance.Log("Seed generation completed in " + randomizerWatch.Elapsed.TotalSeconds + " seconds.");
                    overflow = false;
                    
                }

                else if (!validated)
                {
                    validationWatch.Start();
                    RandomizerMod.Instance.Log("Beginning seed validation...");
                    List<string> floorItems = nonShopItems.Keys.ToList();
                    List<string> shopNames = LogicManager.ShopNames.ToList();
                    List<string> currentItemKeys = new List<string>();
                    List<string> currentItemValues = new List<string>();
                    int passes = 0;
                    while (randomizedItems.Except(currentItemValues).Any())
                    {
                        string[] obtained = currentItemValues.Where(item => !shopNames.Contains(item) && LogicManager.GetItemDef(item).progression).ToArray();
                        foreach (string itemName in floorItems)
                        {
                            if (!currentItemKeys.Contains(itemName) && LogicManager.ParseLogic(itemName, obtained))
                            {
                                currentItemKeys.Add(itemName);
                                currentItemValues.Add(nonShopItems[itemName]);
                            }
                        }
                        foreach (string shopName in shopNames)
                        {
                            if (!currentItemKeys.Contains(shopName) && LogicManager.ParseLogic(shopName, obtained))
                            {
                                currentItemKeys.Add(shopName);
                                foreach (string newItem in shopItems[shopName])
                                {
                                    currentItemValues.Add(newItem);
                                }
                            }
                        }
                        passes++;
                        if (passes > 100) break;
                    }
                    if (passes > 100)
                    {
                        validationWatch.Stop();
                        RandomizerMod.Instance.Log("Failed to validate! Attempting new randomization...");
                        initialized = false;
                        break;
                    }
                    validationWatch.Stop();
                    RandomizerMod.Instance.Log("Seed validation completed in " + validationWatch.Elapsed.TotalSeconds + " seconds.");
                    validated = true;
                }
                else break;
            }

            RandomizerMod.Instance.Log("Finished randomization with " + randomizerAttempts + " attempt(s).");
            RandomizerMod.Instance.Log("Logging progression item placements:");
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
            RandomizerMod.Instance.Log(".");
            RandomizerMod.Instance.Log("Logging ordinary item placements:");
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

            #endregion


            actions = new List<RandomizerAction>();
            int newShinies = 0;

            foreach (KeyValuePair<string, string> kvp in nonShopItems)
            {
                string newItemName = kvp.Value;

                ReqDef oldItem = LogicManager.GetItemDef(kvp.Key);
                ReqDef newItem = LogicManager.GetItemDef(newItemName);

                if (oldItem.replace)
                {
                    actions.Add(new ReplaceObjectWithShiny(oldItem.sceneName, oldItem.objectName, "Randomizer Shiny"));
                    oldItem.objectName = "Randomizer Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }
                else if (oldItem.newShiny)
                {
                    string newShinyName = "New Shiny";
                    if (kvp.Key == "Void_Heart") { }
                    else
                    {
                        newShinyName = "New Shiny " + newShinies++;
                    }
                    actions.Add(new CreateNewShiny(oldItem.sceneName, oldItem.x, oldItem.y, newShinyName));
                    oldItem.objectName = newShinyName;
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }
                else if (oldItem.type == ItemType.Geo && newItem.type != ItemType.Geo)
                {
                    actions.Add(new AddShinyToChest(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, "Randomizer Chest Shiny"));
                    oldItem.objectName = "Randomizer Chest Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }

                string randomizerBoolName = GetAdditiveBoolName(newItemName);
                bool playerdata = false;
                if (string.IsNullOrEmpty(randomizerBoolName))
                {
                    randomizerBoolName = newItem.boolName;
                    playerdata = newItem.type != ItemType.Geo;
                }

                // Dream nail needs a special case
                if (oldItem.boolName == "hasDreamNail")
                {
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Binding Shield Activate", "FSM", "Check", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Plaque Inspect", "Conversation Control", "End", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Scene 2", "Control", "Init", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "PreDreamnail", "FSM", "Check", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "PostDreamnail", "FSM", "Check", randomizerBoolName, playerdata));
                }

                // Good luck to anyone trying to figure out this horrifying switch
                switch (oldItem.type)
                {
                    case ItemType.Charm:
                    case ItemType.Big:
                    case ItemType.Trinket:
                        switch (newItem.type)
                        {
                            case ItemType.Charm:
                            case ItemType.Shop:
                                if (newItem.trinketNum > 0)
                                {
                                    actions.Add(new ChangeShinyIntoTrinket(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.trinketNum));
                                    break;
                                }

                                actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.boolName));
                                if (!string.IsNullOrEmpty(oldItem.altObjectName))
                                {
                                    actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.altObjectName, oldItem.fsmName, newItem.boolName));
                                }

                                break;
                            case ItemType.Big:
                            case ItemType.Spell:
                                BigItemDef[] newItemsArray = GetBigItemDefArray(newItemName);

                                actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItemsArray, randomizerBoolName, playerdata));
                                if (!string.IsNullOrEmpty(oldItem.altObjectName))
                                {
                                    actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.altObjectName, oldItem.fsmName, newItemsArray, randomizerBoolName, playerdata));
                                }

                                break;
                            case ItemType.Geo:
                                if (oldItem.inChest)
                                {
                                    actions.Add(new ChangeChestGeo(oldItem.sceneName, oldItem.chestName, oldItem.chestFsmName, newItem.geo));
                                }
                                else
                                {
                                    actions.Add(new ChangeShinyIntoGeo(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.boolName, newItem.geo));

                                    if (!string.IsNullOrEmpty(oldItem.altObjectName))
                                    {
                                        actions.Add(new ChangeShinyIntoGeo(oldItem.sceneName, oldItem.altObjectName, oldItem.fsmName, newItem.boolName, newItem.geo));
                                    }
                                }

                                break;
                            case ItemType.Trinket:
                                actions.Add(new ChangeShinyIntoTrinket(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.trinketNum));
                                break;

                            default:
                                throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                        }

                        break;
                    case ItemType.Geo:
                        switch (newItem.type)
                        {
                            case ItemType.Geo:
                                actions.Add(new ChangeChestGeo(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.geo));
                                break;
                            default:
                                throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                        }

                        break;
                    default:
                        throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                }

                if (oldItem.cost != 0)
                {
                    actions.Add(new AddYNDialogueToShiny(
                        oldItem.sceneName,
                        oldItem.objectName,
                        oldItem.fsmName,
                        newItem.nameKey,
                        oldItem.cost,
                        oldItem.sceneName == SceneNames.RestingGrounds_07 ? AddYNDialogueToShiny.TYPE_ESSENCE : AddYNDialogueToShiny.TYPE_GEO));
                }
            }

                int shopAdditiveItems = 0;
                List<ChangeShopContents> shopActions = new List<ChangeShopContents>();

                // TODO: Change to use additiveItems rather than hard coded
                // No point rewriting this before making the shop component
                foreach (KeyValuePair<string, List<string>> kvp in shopItems)
                {
                    string shopName = kvp.Key;
                    List<string> newShopItems = kvp.Value;

                    List<ShopItemDef> newShopItemStats = new List<ShopItemDef>();

                    foreach (string item in newShopItems)
                    {
                        ReqDef newItem = LogicManager.GetItemDef(item);

                        if (newItem.type == ItemType.Spell)
                        {
                            switch (newItem.boolName)
                            {
                                case "hasVengefulSpirit":
                                case "hasShadeSoul":
                                    newItem.boolName = "RandomizerMod.ShopFireball" + shopAdditiveItems++;
                                    break;
                                case "hasDesolateDive":
                                case "hasDescendingDark":
                                    newItem.boolName = "RandomizerMod.ShopQuake" + shopAdditiveItems++;
                                    break;
                                case "hasHowlingWraiths":
                                case "hasAbyssShriek":
                                    newItem.boolName = "RandomizerMod.ShopScream" + shopAdditiveItems++;
                                    break;
                                default:
                                    throw new Exception("Unknown spell name: " + newItem.boolName);
                            }
                        }
                        else if (newItem.boolName == "hasDash" || newItem.boolName == "hasShadowDash")
                        {
                            newItem.boolName = "RandomizerMod.ShopDash" + shopAdditiveItems++;
                        }
                        else if (newItem.boolName == nameof(PlayerData.hasDreamNail) || newItem.boolName == nameof(PlayerData.hasDreamGate))
                        {
                            newItem.boolName = "RandomizerMod.ShopDreamNail" + shopAdditiveItems++;
                        }

                        newShopItemStats.Add(new ShopItemDef()
                        {
                            PlayerDataBoolName = newItem.boolName,
                            NameConvo = newItem.nameKey,
                            DescConvo = newItem.shopDescKey,
                            RequiredPlayerDataBool = LogicManager.GetShopDef(shopName).requiredPlayerDataBool,
                            RemovalPlayerDataBool = string.Empty,
                            DungDiscount = LogicManager.GetShopDef(shopName).dungDiscount,
                            NotchCostBool = newItem.notchCost,
                            Cost = 100 + (rand.Next(41) * 10),
                            SpriteName = newItem.shopSpriteKey
                        });
                    }

                    ChangeShopContents existingShopAction = shopActions.Where(action => action.SceneName == LogicManager.GetShopDef(shopName).sceneName && action.ObjectName == LogicManager.GetShopDef(shopName).objectName).FirstOrDefault();

                    if (existingShopAction == null)
                    {
                        shopActions.Add(new ChangeShopContents(LogicManager.GetShopDef(shopName).sceneName, LogicManager.GetShopDef(shopName).objectName, newShopItemStats.ToArray()));
                    }
                    else
                    {
                        existingShopAction.AddItemDefs(newShopItemStats.ToArray());
                    }
                }

                shopActions.ForEach(action => actions.Add(action));
            

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
            ////shopItems.Add("Lemm", new List<string>()); TODO: Custom shop component to handle lemm

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
            obtainedItems = new List<string>();
            storedItems = new List<string>();
            randomizedItems = new List<string>();
            junkStandby = new List<string>();
            progressionStandby = new List<string>();
            locationStandby = new List<string>();

            // Don't place claw in no claw mode, obviously
            if (RandomizerMod.Instance.Settings.NoClaw)
            {
                unobtainedItems.Remove("Mantis_Claw");
            }

            #region Remove fake items
            foreach (string _itemName in LogicManager.ItemNames)
            {
                if (_itemName.StartsWith("LongItemGeo"))
                {
                    unobtainedLocations.Remove(_itemName);
                    unobtainedItems.Remove(_itemName);
                }
            }
            #endregion
            #region Remove long items
            // Handle charms which are too out of the way for normal randomizer
            if (RandomizerMod.Instance.Settings.RandomizeCharms)
            {
                if (RandomizerMod.Instance.Settings.RandomizeLongItems != "Randomized")
                {
                    unobtainedLocations.Remove("Grubberfly's_Elegy");
                    unobtainedLocations.Remove("King_Fragment");
                }
                if (RandomizerMod.Instance.Settings.RandomizeLongItems == "Bonus Geo")
                {
                    nonShopItems.Add("Grubberfly's_Elegy", "LongItemGeo1");
                    nonShopItems.Add("King_Fragment", "LongItemGeo2");
                }
                else if (RandomizerMod.Instance.Settings.RandomizeLongItems == "Vanilla")
                {
                    unobtainedItems.Remove("Grubberfly's_Elegy");
                    unobtainedItems.Remove("King_Fragment");
                }
            }
            #endregion
            #region Remove non-randomized pools

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
                        if (item.progression) storedItems.Add(_itemName);
                    }
                }
                RandomizerMod.Instance.Log("Geo Chests left in vanilla locations.");
            }
            #endregion

            geoItems = unobtainedItems.Where(name => LogicManager.GetItemDef(name).type == ItemType.Geo).ToList();
            randomizedItems = unobtainedLocations.Where(name => !LogicManager.ShopNames.Contains(name) && LogicManager.GetItemDef(name).type != ItemType.Geo).ToList();

            shopMax = unobtainedItems.Count - unobtainedLocations.Count + 5;

            if (shopMax < 5)
            {
                foreach (string shopName in LogicManager.ShopNames) unobtainedLocations.Remove(shopName);
            }
            reachableShops = LogicManager.ShopNames.ToList();
            reachableShops.Remove("Sly_(Key)");

            randomizerAttempts = 0;
            initialized = false;
            randomized = false;
            overflow = false;
            validated = false;
            Done = false;
        }

        private static List<string> GetProgressionItems(int reachableCount)
        {
            List<string> progression = new List<string>();
            string[] obtained = new string[obtainedItems.Count + 1];
            obtainedItems.CopyTo(obtained);

            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).progression)
                {
                    obtained[obtained.Length - 1] = str;

                    int hypothetical = 0;
                    foreach (string item in unobtainedLocations)
                    {
                        if (LogicManager.ParseLogic(item, obtained))
                        {
                            hypothetical++;
                        }
                    }

                    if (hypothetical > reachableCount)
                    {
                        progression.Add(str);
                    }
                }
            }

            return progression;
        }

        private static List<string> GetCandidateItems()
        {
            List<string> progression = new List<string>();

            foreach (string str in unobtainedItems)
            {
                // Baldur kills and Sprintmaster/Dashmaster are never good candidates, so we don't add them
                if (str == "Mark_of_Pride" || str == "Longnail" || str == "Spore_Shroom" || str == "Glowing_Womb" || str == "Grubberfly's_Elegy" || str == "Weaversong" || str == "Sprintmaster" || str == "Dashmaster") { }
                // Remove redundant items
                else if (str == "Shopkeeper's_Key" || str == "Void_Heart" || str == "Shade_Soul" || str == "Abyss_Shriek" || str == "Descending_Dark" || str == "Dream_Gate") { }
                else if (LogicManager.GetItemDef(str).progression)
                {
                    progression.Add(str);
                }
            }

            return progression;
        }

        private static string GetAdditivePrefix(string boolName)
        {
            foreach (string itemSet in LogicManager.AdditiveItemNames)
            {
                if (LogicManager.GetAdditiveItems(itemSet).Contains(boolName))
                {
                    return itemSet;
                }
            }

            return null;
        }

        private static BigItemDef[] GetBigItemDefArray(string boolName)
        {
            string prefix = GetAdditivePrefix(boolName);
            if (prefix != null)
            {
                List<BigItemDef> itemDefs = new List<BigItemDef>();
                foreach (string str in LogicManager.GetAdditiveItems(prefix))
                {
                    ReqDef item = LogicManager.GetItemDef(str);
                    itemDefs.Add(new BigItemDef()
                    {
                        BoolName = item.boolName,
                        SpriteKey = item.bigSpriteKey,
                        TakeKey = item.takeKey,
                        NameKey = item.nameKey,
                        ButtonKey = item.buttonKey,
                        DescOneKey = item.descOneKey,
                        DescTwoKey = item.descTwoKey
                    });
                }

                return itemDefs.ToArray();
            }
            else
            {
                ReqDef item = LogicManager.GetItemDef(boolName);
                return new BigItemDef[]
                {
                    new BigItemDef()
                    {
                        BoolName = item.boolName,
                        SpriteKey = item.bigSpriteKey,
                        TakeKey = item.takeKey,
                        NameKey = item.nameKey,
                        ButtonKey = item.buttonKey,
                        DescOneKey = item.descOneKey,
                        DescTwoKey = item.descTwoKey
                    }
                };
            }
        }

        private static string GetAdditiveBoolName(string boolName)
        {
            if (additiveCounts == null)
            {
                additiveCounts = new Dictionary<string, int>();
                foreach (string str in LogicManager.AdditiveItemNames)
                {
                    additiveCounts.Add(str, 0);
                }
            }

            string prefix = GetAdditivePrefix(boolName);
            if (!string.IsNullOrEmpty(prefix))
            {
                additiveCounts[prefix] = additiveCounts[prefix] + 1;
                return prefix + additiveCounts[prefix];
            }

            return null;
        }

        private static void LogItemPlacement(string item, string location)
        {
            RandomizerMod.Instance.Settings.itemPlacements.Add(item, location);
            RandomizerMod.Instance.Log($"Putting item \"{item.Replace('_', ' ')}\" at \"{location.Replace('_', ' ')}\"");
        }
    }
}
