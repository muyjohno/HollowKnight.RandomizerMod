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
        private static ItemManager itemManager;
        private static TransitionManager transitionManager;

        private static bool overflow;
        private static bool firstPassDone;
        private static bool randomizationError;
        public static bool Done { get; private set; }
        private static Random rand = new Random(RandomizerMod.Instance.Settings.Seed);

        public static void RandomizeTransitions()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Log("Beginning transition randomization");
            while (true)
            {
                SetupTransitionVariables();
                if (RandomizerMod.Instance.Settings.RandomizeAreas) BuildAreaSpanningTree();
                else BuildRoomSpanningTree();
                PlaceSpecialTransitions();
                ConnectStartToGraph();
                CompleteTransitionGraph();
                if (ValidateTransitionRandomization()) break;
                if (randomizationError)
                {
                    Log("Error encountered while randomizing transitions, attempting again...");
                }
            }
            watch.Stop();
            RandomizerMod.Instance.Log("Transition randomization finished in " + watch.Elapsed.TotalSeconds + " seconds.");
        }
        public static void RandomizeItems()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                SetupItemVariables();
                PlaceLongItems();
                HandleFakeItems();
                if (RandomizerMod.Instance.Settings.RandomizeTransitions) PlaceFury();
                PlaceGeoItems();
                FirstPass();
                SecondPass();
                if (ValidateItemRandomization()) break;
            }
            SaveAllPlacements();
            SaveItemHints();
            if (RandomizerMod.Instance.Settings.CreateSpoilerLog) RandoLogger.LogAllToSpoiler(RandomizerMod.Instance.Settings.ItemPlacements, RandomizerMod.Instance.Settings._transitionPlacements.Select(kvp => (kvp.Key, kvp.Value)).ToArray());
            
            Done = true;
            watch.Stop();
            RandomizerMod.Instance.Log("Item randomization finished in " + watch.Elapsed.TotalSeconds + " seconds.");
        }

        

        private static void SetupTransitionVariables()
        {
            randomizationError = false;

            transitionManager = new TransitionManager(rand);
            itemManager = new ItemManager(rand);
        }
        private static void SetupItemVariables()
        {
            itemManager = new ItemManager(rand);

            firstPassDone = false;
            overflow = false;
            Done = false;
        }

        private static void BuildAreaSpanningTree()
        {
            List<string> areas = new List<string>();
            Dictionary<string, List<string>> areaTransitions = new Dictionary<string, List<string>>();

            foreach (string transition in LogicManager.TransitionNames())
            {
                TransitionDef def = LogicManager.GetTransitionDef(transition);
                string areaName = def.areaName;
                if (areaName == "Kings_Pass") continue;
                if (new List<string> { "Dirtmouth", "Forgotten_Crossroads", "Resting_Grounds" }.Contains(areaName)) areaName = "Kings_Station";
                if (new List<string> { "Ancient_Basin", "Kingdoms_Edge" }.Contains(areaName)) areaName = "Deepnest";

                if (!areas.Contains(areaName) && !def.deadEnd && !def.isolated)
                {
                    areas.Add(areaName);
                    areaTransitions.Add(areaName, new List<string>());
                }
            }

            foreach (string transition in LogicManager.TransitionNames())
            {
                TransitionDef def = LogicManager.GetTransitionDef(transition);
                string areaName = def.areaName;
                if (def.oneWay == 0 && areas.Contains(areaName)) areaTransitions[areaName].Add(transition);
            }

            List<string> remainingAreas = areas;
            string firstArea = "Kings_Station";
            remainingAreas.Remove(firstArea);
            DirectedTransitions directed = new DirectedTransitions(rand);
            directed.Add(areaTransitions[firstArea].Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList());
            int failsafe = 0;

            while (remainingAreas.Any())
            {
                failsafe++;
                if (failsafe > 100)
                {
                    randomizationError = true;
                    return;
                }

                string nextArea = remainingAreas[rand.Next(remainingAreas.Count)];
                List<string> nextAreaTransitions = areaTransitions[nextArea].Where(transition => !LogicManager.GetTransitionDef(transition).deadEnd && directed.Test(transition)).ToList();
                if (nextAreaTransitions.Count < 1) continue;

                string transitionTarget = nextAreaTransitions[rand.Next(nextAreaTransitions.Count)];
                string transitionSource = directed.GetNextTransition(transitionTarget);
                transitionManager.PlaceTransitionPair(transitionSource, transitionTarget);
                remainingAreas.Remove(nextArea);

                List<string> newTransitions = areaTransitions[nextArea].Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList();
                directed.Add(newTransitions);
                directed.Remove(transitionTarget, transitionSource);
            }
        }

        private static void BuildRoomSpanningTree()
        {
            List<string> rooms = new List<string>();
            Dictionary<string, List<string>> roomTransitions = new Dictionary<string, List<string>>();

            foreach (string transition in LogicManager.TransitionNames())
            {
                TransitionDef def = LogicManager.GetTransitionDef(transition);
                string roomName = def.sceneName;
                if (roomName == "Tutorial_01") continue;
                if (new List<string> { "Crossroads_46", "Crossroads_46b" }.Contains(roomName)) roomName = "Crossroads_46";
                if (new List<string> { "Abyss_03", "Abyss_03_b", "Abyss_03_c" }.Contains(roomName)) roomName = "Abyss_03";
                if (new List<string> { "Ruins2_10", "Ruins2_10b" }.Contains(roomName)) roomName = "Ruins2_10";

                if (!rooms.Contains(roomName) && !def.deadEnd && !def.isolated)
                {
                    rooms.Add(roomName);
                    roomTransitions.Add(roomName, new List<string>());
                }
            }

            foreach (string transition in LogicManager.TransitionNames())
            {
                TransitionDef def = LogicManager.GetTransitionDef(transition);
                string roomName = def.sceneName;
                if (def.oneWay == 0 && rooms.Contains(roomName)) roomTransitions[roomName].Add(transition);
            }

            List<string> remainingRooms = rooms;
            string firstRoom = "Ruins2_04";
            remainingRooms.Remove(firstRoom);
            DirectedTransitions directed = new DirectedTransitions(rand);
            directed.Add(roomTransitions[firstRoom].Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList());
            int failsafe = 0;

            while (remainingRooms.Any())
            {
                failsafe++;
                if (failsafe > 500)
                {
                    randomizationError = true;
                    return;
                }

                string nextRoom = remainingRooms[rand.Next(remainingRooms.Count)];
                List<string> nextAreaTransitions = roomTransitions[nextRoom].Where(transition => !LogicManager.GetTransitionDef(transition).deadEnd && directed.Test(transition)).ToList();
                if (nextAreaTransitions.Count < 1) continue;

                string transitionTarget = nextAreaTransitions[rand.Next(nextAreaTransitions.Count)];
                string transitionSource = directed.GetNextTransition(transitionTarget);
                transitionManager.PlaceTransitionPair(transitionSource, transitionTarget);
                remainingRooms.Remove(nextRoom);

                List<string> newTransitions = roomTransitions[nextRoom].Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList();
                directed.Add(newTransitions);
                directed.Remove(transitionTarget, transitionSource);
            }
        }

        private static void PlaceSpecialTransitions()
        {
            if (randomizationError) return;
            List<string> oneWayEntrances = LogicManager.TransitionNames().Where(transition => LogicManager.GetTransitionDef(transition).oneWay == 1).ToList();
            List<string> oneWayExits = LogicManager.TransitionNames().Where(transition => LogicManager.GetTransitionDef(transition).oneWay == 2).ToList();
            List<string> horizontalOneWays = oneWayEntrances.Where(t => !LogicManager.GetTransitionDef(t).doorName.StartsWith("b")).ToList();

            while (horizontalOneWays.Any())
            {
                string horizontalEntrance = horizontalOneWays.First();
                string downExit = oneWayExits[rand.Next(oneWayExits.Count)];
                transitionManager.PlaceOneWayPair(horizontalEntrance, downExit);
                oneWayEntrances.Remove(horizontalEntrance);
                horizontalOneWays.Remove(horizontalEntrance);
                oneWayExits.Remove(downExit);
            }

            DirectedTransitions directed = new DirectedTransitions(rand);
            directed.Add(oneWayExits);
            while (oneWayEntrances.Any())
            {
                string entrance = oneWayEntrances[rand.Next(oneWayEntrances.Count)];
                string exit = directed.GetNextTransition(entrance);
                transitionManager.PlaceOneWayPair(entrance, exit);
                oneWayEntrances.Remove(entrance);
                oneWayExits.Remove(exit);
                directed.Remove(exit);
            }

            List<string> isolatedTransitions = transitionManager.unplacedTransitions.Where(transition => LogicManager.GetTransitionDef(transition).isolated).ToList();
            List<string> nonisolatedTransitions = transitionManager.unplacedTransitions.Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList();
            directed = new DirectedTransitions(rand);
            directed.Add(nonisolatedTransitions);
            directed.Remove("Tutorial_01[right1]", "Tutorial_01[top2]");
            while (isolatedTransitions.Any())
            {
                string transition1 = isolatedTransitions[rand.Next(isolatedTransitions.Count)];
                string transition2 = directed.GetNextTransition(transition1);
                transitionManager.PlaceStandbyPair(transition1, transition2);
                isolatedTransitions.Remove(transition1);
                directed.Remove(transition2);
            }
        }

        private static void ConnectStartToGraph()
        {
            if (randomizationError) return;
            Log("Attaching King's Pass to graph...");

            transitionManager.pm = new ProgressionManager();
            itemManager.ResetReachableLocations();
            transitionManager.ResetReachableTransitions();

            if (true) // keeping local variables out of the way
            {
                IEnumerable<string> baseTransitions = transitionManager.GetProgressionTransitions().Where(t => LogicManager.GetTransitionDef(t).doorName.StartsWith("l"));
                // We place a random item at fotf, and then force transitions which unlock new transitions until we reach a place with more item locations

                List<string> progressionItems = itemManager.areaCandidateItems;
                progressionItems.Remove("Mantis_Claw");
                progressionItems.Remove("Monarch_Wings");
                string placeItem = progressionItems[rand.Next(progressionItems.Count)];
                transitionManager.pm.Add(placeItem);

                List<string> progressionTransitions = transitionManager.GetProgressionTransitions().Where(t => LogicManager.GetTransitionDef(t).doorName.StartsWith("l")).ToList();
                if (progressionTransitions.Count == 0) // this should happen extremely rarely, but it has to be handled
                {
                    Log("No way out of King's Pass?!?");
                    randomizationError = true;
                    return;
                }

                IEnumerable<string> newTransitions = progressionTransitions.Except(baseTransitions);
                string transition1 = "Tutorial_01[right1]";
                string transition2 = progressionTransitions[rand.Next(progressionTransitions.Count)];
                if (!newTransitions.Contains(transition2)) transition2 = progressionTransitions[rand.Next(progressionTransitions.Count)];
                if (!newTransitions.Contains(transition2)) transition2 = progressionTransitions[rand.Next(progressionTransitions.Count)]; // Heavy weight towards following up on a first transition unlocked by fotf

                itemManager.PlaceItem(placeItem, "Fury_of_the_Fallen");
                transitionManager.PlaceTransitionPair(transition1, transition2);
            }


            while (true)
            {
                transitionManager.UpdateReachableTransitions();
                transitionManager.UnloadReachableStandby();
                itemManager.UpdateReachableLocations(transitionManager.pm);
                if (itemManager.availableCount > 0)
                {
                    return;
                }
                
                List<string> progressionTransitions = transitionManager.GetProgressionTransitions();
                if (progressionTransitions.Count > 0)
                {
                    DirectedTransitions directed = new DirectedTransitions(rand);
                    directed.Add(progressionTransitions);
                    List<string> placeableTransitions = transitionManager.reachableTransitions.Intersect(transitionManager.unplacedTransitions.Union(transitionManager.standbyTransitions.Keys)).Where(tr => directed.Test(tr)).ToList();
                    if (placeableTransitions.Count > 0)
                    {
                        string transition1 = placeableTransitions[rand.Next(placeableTransitions.Count)];
                        string transition2 = directed.GetNextTransition(transition1);
                        transitionManager.PlaceTransitionPair(transition1, transition2);
                        continue;
                    }
                    else
                    {
                        Log("Could not connect King's Pass to map--ran out of placeable transitions.");
                        foreach (string t in transitionManager.GetReachableTransitions().Intersect(transitionManager.unplacedTransitions.Union(transitionManager.standbyTransitions.Keys))) Log("Can place: " + t);
                        randomizationError = true;
                        return;
                    }
                }
                else
                {
                    Log("Could not connect King's Pass to map--ran out of progression transitions.");
                    randomizationError = true;
                    return;
                }
            }
        }

        private static void CompleteTransitionGraph()
        {
            if (randomizationError) return;
            int failsafe = 0;
            Log("Beginning full placement of transitions...");

            while (transitionManager.unplacedTransitions.Any())
            {
                failsafe++;
                if (failsafe > 100)
                {
                    Log("Aborted randomization on too many passes. At the time, there were:");
                    Log("Unplaced transitions: " + transitionManager.unplacedTransitions.Count);
                    Log("Reachable transitions: " + transitionManager.reachableTransitions.Count);
                    Log("Reachable unplaced transitions, directionally compatible: " + transitionManager.placeableCount);
                    Log("Reachable item locations: " + itemManager.reachableCount);
                    foreach (string t in transitionManager.unplacedTransitions) Log(t);
                    randomizationError = true;
                    return;
                }

                if (itemManager.availableCount < 1) itemManager.UpdateReachableLocations(transitionManager.pm);
                if (itemManager.availableCount > 1)
                {
                    if (itemManager.areaCandidateCount > 0)
                    {
                        string placeLocation = itemManager.availableLocations[rand.Next(itemManager.availableLocations.Count)];
                        string placeItem = itemManager.areaCandidateItems[rand.Next(itemManager.areaCandidateCount)];
                        itemManager.PlaceItem(placeItem, placeLocation);
                        transitionManager.pm.Add(placeItem);
                    }
                }

                
                int placeableCount = transitionManager.placeableCount;
                if (placeableCount < 4) transitionManager.UpdateReachableTransitions();

                if (placeableCount == 0 && itemManager.reachableCount == 0)
                {
                    Log("Ran out of locations?!?");
                    randomizationError = true;
                    return;
                }
                else if (placeableCount > 2)
                {
                    transitionManager.UnloadReachableStandby();
                    string transition1 = transitionManager.placeableTransitions[rand.Next(placeableCount)];
                    string transition2 = transitionManager.dt.GetNextTransition(transition1);
                    transitionManager.PlaceTransitionPair(transition1, transition2);
                    continue;
                }
                else if (transitionManager.unplacedTransitions.Count == 2)
                {
                    string transition1 = transitionManager.unplacedTransitions[0];
                    string transition2 = transitionManager.unplacedTransitions[1];
                    transitionManager.PlaceTransitionPair(transition1, transition2);
                    continue;
                }
                else if (placeableCount != 0)
                {
                    List<string> progressionTransitions = transitionManager.GetProgressionTransitions();
                    if (progressionTransitions.Count > 0)
                    {
                        DirectedTransitions directed = new DirectedTransitions(rand);
                        directed.Add(progressionTransitions);
                        bool placed = false;
                        foreach (string transition1 in transitionManager.placeableTransitions)
                        {
                            if (directed.Test(transition1))
                            {
                                string transition2 = directed.GetNextTransition(transition1);
                                transitionManager.PlaceTransitionPair(transition1, transition2);
                                placed = true;
                                break;
                            }
                        }
                        if (placed) continue;
                    }
                }

                itemManager.UpdateReachableLocations(transitionManager.pm);
                if (itemManager.reachableCount > 0)
                {
                    List<string> progressionItems = itemManager.GetProgressionItems(transitionManager.pm);

                    if (progressionItems.Count > 0)
                    {
                        string placeItem = progressionItems[rand.Next(progressionItems.Count)];
                        string placeLocation = itemManager.availableLocations[rand.Next(itemManager.availableLocations.Count)];
                        itemManager.PlaceItem(placeItem, placeLocation);
                        transitionManager.pm.Add(placeItem);
                        continue;
                    }
                    // Last ditch effort to save the seed. The list is ordered by which items are heuristically likely to unlock transitions at this point.
                    else
                    {
                        foreach (string placeItem in new List<string> { "Mantis_Claw", "Monarch_Wings", "Desolate_Dive", "Isma's_Tear", "Crystal_Heart", "Mothwing_Cloak", "Shade_Cloak" })
                        {
                            if (!transitionManager.pm.Has(placeItem))
                            {
                                string placeLocation = itemManager.availableLocations[rand.Next(itemManager.availableLocations.Count)];
                                itemManager.PlaceItem(placeItem, placeLocation);
                                transitionManager.pm.Add(placeItem);
                                break;
                            }
                        }
                    }
                }
            }
            transitionManager.UnloadStandby();
        }

        private static void PlaceLongItems()
        {
            List<string> locations = itemManager.unobtainedLocations.Except(LogicManager.ShopNames).ToList();
            int eggCount = 1;
            foreach (string location in locations)
            {
                if (LogicManager.GetItemDef(location).longItemTier > RandomizerMod.Instance.Settings.LongItemTier)
                {
                    itemManager.PlaceItem("Bonus_Arcane_Egg_(" + eggCount + ")", location);
                    eggCount++;
                }
            }
        }

        private static void HandleFakeItems()
        {
            itemManager.RemoveFakeItems();
            if (RandomizerMod.Instance.Settings.PleasureHouse) itemManager.PlaceItem("Small_Reward_Geo", "Pleasure_House");
        }

        private static void PlaceFury()
        {
            string placeItem;
            string placeLocation = "Fury_of_the_Fallen";

            List<string> reachableLocations = itemManager.GetReachableLocations().Intersect(itemManager.unobtainedLocations).ToList();
            if (reachableLocations.Count == 1)
            {
                List<string> progressionItems = itemManager.GetProgressionItems();
                progressionItems.Remove("Mantis_Claw");
                progressionItems.Remove("Monarch_Wings");
                if (progressionItems.Count > 0)
                {
                    placeItem = progressionItems[rand.Next(progressionItems.Count)];
                }
                else return; // leave it to go into overflow during firstpass
            }
            else
            {
                placeItem = itemManager.areaCandidateItems[rand.Next(itemManager.areaCandidateItems.Count)];
                while(placeItem == "Mantis_Claw" || placeItem == "Monarch_Wings") placeItem = itemManager.areaCandidateItems[rand.Next(itemManager.areaCandidateItems.Count)];
            }
            itemManager.PlaceItem(placeItem, placeLocation);
        }
        private static void PlaceGeoItems()
        {
            List<string> geoLocations = itemManager.unobtainedLocations.Except(itemManager.GetReachableLocations()).Except(LogicManager.ShopNames).Where(location => LogicManager.GetItemDef(location).cost == 0).ToList();
            // Place geo pickups outside shops and toll locations
            while(itemManager.geoItems.Any())
            {
                string placeItem = itemManager.geoItems[rand.Next(itemManager.geoItems.Count)];
                string placeLocation = geoLocations[rand.Next(geoLocations.Count)];
                itemManager.PlaceItem(placeItem, placeLocation);
                geoLocations.Remove(placeLocation);
            }
        }

        private static void FirstPass()
        {
            Log("Beginning first pass of item placement...");
            itemManager.ResetReachableLocations();
            while (!firstPassDone)
            {
                string placeItem;
                string placeLocation;
                
                switch (itemManager.availableCount)
                {
                    case 0:
                        firstPassDone = true;
                        return;
                    case 1:
                        List<string> progressionItems = itemManager.GetProgressionItems(); // Progression items which open new locations
                        List<string> itemCandidateItems = itemManager.itemCandidateItems;
                        if (progressionItems.Count > 0)
                        {
                            placeLocation = itemManager.availableLocations.First();
                            placeItem = progressionItems[rand.Next(progressionItems.Count)];
                        }
                        else if (itemManager.unobtainedLocations.Count > 1 && itemCandidateItems.Count > 0)
                        {
                            overflow = true;
                            placeItem = itemCandidateItems[rand.Next(itemCandidateItems.Count)];
                            itemManager.PlaceProgressionToStandby(placeItem);
                            continue;
                        }
                        else // This is how the last reachable location is filled
                        {
                            placeItem = itemManager.unobtainedItems[rand.Next(itemManager.unobtainedItems.Count)];
                            placeLocation = itemManager.availableLocations.First();
                        }
                        break;
                    default:
                        placeItem = itemManager.unobtainedItems[rand.Next(itemManager.unobtainedItems.Count)];
                        placeLocation = itemManager.availableLocations[rand.Next(itemManager.availableLocations.Count)];
                        break;
                }

                if (!overflow && !LogicManager.GetItemDef(placeItem).progression)
                {
                    itemManager.PlaceJunkItemToStandby(placeItem, placeLocation);
                }
                else
                {
                    itemManager.PlaceItem(placeItem, placeLocation);
                }
            }
        }

        private static void SecondPass()
        {
            Log("Beginning second pass of item placement...");
            foreach (string placeItem in itemManager.junkStandby) itemManager.unobtainedItems.Add(placeItem);

            // First, we have to guarantee that items used in the logic chain are accessible
            foreach (string placeItem in itemManager.progressionStandby)
            {
                string placeLocation;
                if (itemManager.locationStandby.Count > 0)
                {
                    placeLocation = itemManager.locationStandby[rand.Next(itemManager.locationStandby.Count)];
                }
                else
                {
                    placeLocation = LogicManager.ShopNames[rand.Next(5)];
                }
                itemManager.PlaceItemFromStandby(placeItem, placeLocation);
            }

            // We fill the remaining locations and shops with the leftover junk
            while (itemManager.unobtainedItems.Count > 0)
            {
                string placeItem = itemManager.unobtainedItems[rand.Next(itemManager.unobtainedItems.Count)];
                string placeLocation;

                if (itemManager.unobtainedLocations.Count > 0)
                {
                    placeLocation = itemManager.unobtainedLocations[rand.Next(itemManager.unobtainedLocations.Count)];
                }
                else if (itemManager.locationStandby.Count > 0)
                {
                    placeLocation = itemManager.locationStandby[rand.Next(itemManager.locationStandby.Count)];
                }
                else
                {
                    placeLocation = LogicManager.ShopNames[rand.Next(5)];
                }
                itemManager.PlaceItemFromStandby(placeItem, placeLocation);
            }
        }

        private static bool ValidateTransitionRandomization()
        {
            if (randomizationError) return false;
            Log("Beginning transition placement validation...");

            ProgressionManager pm = new ProgressionManager();
            foreach (string item in LogicManager.ItemNames.Where(i => LogicManager.GetItemDef(i).progression)) pm.Add(item);

            List<string> remainingTransitions = LogicManager.TransitionNames().ToList();
            int failsafe = 0;
            while (remainingTransitions.Any())
            {
                remainingTransitions = remainingTransitions.Except(transitionManager.GetReachableTransitions(pm)).ToList();
                failsafe++;
                if (failsafe > 200)
                {
                    randomizationError = true;
                    Log("Transition placements failed to validate!");
                    foreach (string t in remainingTransitions) Log(t + ", " + TransitionManager.transitionPlacements.FirstOrDefault(kvp =>kvp.Key == t).Value);
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateItemRandomization()
        {
            RandomizerMod.Instance.Log("Beginning seed validation...");
            List<string> floorItems = ItemManager.nonShopItems.Where(kvp => !LogicManager.GetItemDef(kvp.Value).isFake).Select(kvp => kvp.Key).ToList();
            List<string> everything = new List<string>();
            everything.AddRange(floorItems);
            everything.AddRange(LogicManager.ShopNames);
            if (RandomizerMod.Instance.Settings.RandomizeTransitions) everything.AddRange(LogicManager.TransitionNames());

            ProgressionManager pm = new ProgressionManager();
            int passes = 0;
            while (everything.Any())
            {
                if (RandomizerMod.Instance.Settings.RandomizeTransitions) everything = everything.Except(transitionManager.GetReachableTransitions(pm)).ToList();
                foreach (string location in floorItems)
                {
                    if (everything.Contains(location) && pm.CanGet(location))
                    {
                        everything.Remove(location);
                        if (LogicManager.GetItemDef(ItemManager.nonShopItems[location]).progression) pm.Add(ItemManager.nonShopItems[location]);
                    }
                }
                foreach (string shop in LogicManager.ShopNames)
                {
                    if (everything.Contains(shop) && pm.CanGet(shop))
                    {
                        everything.Remove(shop);
                        foreach (string newItem in ItemManager.shopItems[shop])
                        {
                            if (LogicManager.GetItemDef(newItem).progression) pm.Add(newItem);
                        }
                    }
                }

                passes++;
                if (passes > 400)
                {
                    Log("Unable to validate!");
                    Log("Able to get: " + pm.ListObtainedProgression());
                    string m = string.Empty;
                    foreach (string s in everything) m += s + ", ";
                    Log("Unable to get: " + m);
                    return false;
                }
            }
            Log("Validation successful.");
            return true;
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
                    RandomizerMod.Instance.Settings.AddItemPlacement(item, kvp.Key);
                }
            }
            foreach (KeyValuePair<string, string> kvp in ItemManager.nonShopItems)
            {
                RandomizerMod.Instance.Settings.AddItemPlacement(kvp.Value, kvp.Key);
            }
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
    }
}