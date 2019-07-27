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
        private static Random rand;

        public static void Randomize()
        {
            RandomizerMod.Instance.Settings.ResetPlacements();
            rand = new Random(RandomizerMod.Instance.Settings.Seed);

            RandomizeCosts();
            if (RandomizerMod.Instance.Settings.RandomizeTransitions) RandomizeTransitions();
            RandomizeItems();

            RandomizerAction.CreateActions(RandomizerMod.Instance.Settings.ItemPlacements, RandomizerMod.Instance.Settings.Seed);
        }

        public static void RandomizeCosts()
        {
            foreach (string item in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(item).costType == 1) //essence cost
                {
                    ReqDef def = LogicManager.GetItemDef(item);
                    int cost = 1 + rand.Next(900);
                    def.cost = cost;
                    LogicManager.EditItemDef(item, def);
                    RandomizerMod.Instance.Settings.AddNewCost(item, cost);
                }

                if (LogicManager.GetItemDef(item).costType == 3) //grub cost
                {
                    ReqDef def = LogicManager.GetItemDef(item);
                    int cost = 1 + rand.Next(23);
                    def.cost = cost;
                    LogicManager.EditItemDef(item, def);
                    RandomizerMod.Instance.Settings.AddNewCost(item, cost);
                }
            }
        }

        public static void RandomizeTransitions()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            
            while (true)
            {
                Log("");
                Log("Beginning transition randomization...");
                SetupTransitionVariables();
                if (RandomizerMod.Instance.Settings.RandomizeAreas) BuildAreaSpanningTree();
                else if (RandomizerMod.Instance.Settings.RandomizeRooms && RandomizerMod.Instance.Settings.ConnectAreas) BuildCARSpanningTree();
                else if (RandomizerMod.Instance.Settings.RandomizeRooms && !RandomizerMod.Instance.Settings.ConnectAreas) BuildRoomSpanningTree();
                else
                {
                    LogWarn("Ambiguous settings passed to transition randomizer, skipping and continuing on to item randomizer...");
                    return;
                }

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
                Log("");
                Log("Beginning item randomization...");
                SetupItemVariables();
                if (RandomizerMod.Instance.Settings.RandomizeTransitions) PlaceFury();
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

            BuildSpanningTree(areaTransitions);
            PlaceOneWayTransitions();
            PlaceIsolatedTransitions();
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

            BuildSpanningTree(roomTransitions);
            PlaceOneWayTransitions();
            PlaceIsolatedTransitions();
        }

        private static void BuildCARSpanningTree()
        {
            PlaceOneWayTransitions();
            List<string> areas = new List<string>();
            Dictionary<string, List<string>> rooms = new Dictionary<string, List<string>>();
            foreach (string t in transitionManager.unplacedTransitions)
            {
                if (!LogicManager.GetTransitionDef(t).isolated || !LogicManager.GetTransitionDef(t).deadEnd)
                {
                    if (!areas.Contains(LogicManager.GetTransitionDef(t).areaName))
                    {
                        areas.Add(LogicManager.GetTransitionDef(t).areaName);
                        rooms.Add(LogicManager.GetTransitionDef(t).areaName, new List<string>());
                    }


                    if (!rooms[LogicManager.GetTransitionDef(t).areaName].Contains(LogicManager.GetTransitionDef(t).sceneName))
                        rooms[LogicManager.GetTransitionDef(t).areaName].Add(LogicManager.GetTransitionDef(t).sceneName);
                }
            }

            var areaTransitions = new Dictionary<string, Dictionary<string, List<string>>>(); // [area][scene][transition]
            foreach (string area in areas) areaTransitions.Add(area, new Dictionary<string, List<string>>());
            foreach (var kvp in rooms) foreach (string room in kvp.Value) areaTransitions[kvp.Key].Add(room, new List<string>());
            foreach (string t in transitionManager.unplacedTransitions)
            {
                TransitionDef def = LogicManager.GetTransitionDef(t);
                if (!areas.Contains(def.areaName) || !areaTransitions[def.areaName].ContainsKey(def.sceneName)) continue;
                areaTransitions[def.areaName][def.sceneName].Add(t);
            }
            foreach (string area in areas) BuildSpanningTree(areaTransitions[area]);
            var worldTransitions = new Dictionary<string, List<string>>();
            foreach (string area in areas)
            {
                if (area == "Kings_Pass") continue;
                worldTransitions.Add(area, new List<string>());
            }
            foreach (string t in transitionManager.unplacedTransitions)
            {
                if (t.StartsWith("Tut")) continue;
                if (areas.Contains(LogicManager.GetTransitionDef(t).areaName) && rooms[LogicManager.GetTransitionDef(t).areaName].Contains(LogicManager.GetTransitionDef(t).sceneName))
                {
                    worldTransitions[LogicManager.GetTransitionDef(t).areaName].Add(t);
                }
            }
            BuildSpanningTree(worldTransitions);
            PlaceIsolatedTransitions();
        }

        private static void BuildSpanningTree(Dictionary<string, List<string>> sortedTransitions, string first = null)
        {
            List<string> remaining = sortedTransitions.Keys.ToList();
            while (first == null)
            {
                first = remaining[rand.Next(remaining.Count)];
                if (!sortedTransitions[first].Any(t => !LogicManager.GetTransitionDef(t).isolated)) first = null;
            }
            remaining.Remove(first);
            List<DirectedTransitions> directed = new List<DirectedTransitions>();
            directed.Add(new DirectedTransitions(rand));
            directed[0].Add(sortedTransitions[first].Where(t => !LogicManager.GetTransitionDef(t).isolated).ToList());
            int failsafe = 0;

            while (remaining.Any())
            {
                bool placed = false;
                failsafe++;
                if (failsafe > 500 || !directed[0].AnyCompatible())
                {
                    Log("Triggered failsafe on round " + failsafe + " in BuildSpanningTree, where first transition set was: " + first + " with count: " + sortedTransitions[first].Count);
                    randomizationError = true;
                    return;
                }

                string nextRoom = remaining[rand.Next(remaining.Count)];

                    foreach (DirectedTransitions dt in directed)
                {
                    List<string> nextAreaTransitions = sortedTransitions[nextRoom].Where(transition => !LogicManager.GetTransitionDef(transition).deadEnd && dt.Test(transition)).ToList();
                    List<string> newTransitions = sortedTransitions[nextRoom].Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList();

                    if (!nextAreaTransitions.Any())
                    {
                        continue;
                    }

                    string transitionTarget = nextAreaTransitions[rand.Next(nextAreaTransitions.Count)];
                    string transitionSource = dt.GetNextTransition(transitionTarget);

                    transitionManager.PlaceTransitionPair(transitionSource, transitionTarget);
                    remaining.Remove(nextRoom);

                    dt.Add(newTransitions);
                    dt.Remove(transitionTarget, transitionSource);
                    placed = true;
                    break;
                }
                if (placed) continue;
                else
                {
                    DirectedTransitions dt = new DirectedTransitions(rand);
                    dt.Add(sortedTransitions[nextRoom].Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList());
                    directed.Add(dt);
                    remaining.Remove(nextRoom);
                }
            }
            //Log("Completed first pass of BuildSpanningTree with " + directed.Count + " connected component(s).");
            for (int i=0; i<directed.Count; i++)
            {
                DirectedTransitions dt = directed[i];
                DirectedTransitions dt1 = null;
                string transition1 = null;
                string transition2 = null;

                foreach (var dt2 in directed)
                {
                    if (dt == dt2) continue;

                    if (dt.left && dt2.right)
                    {
                        transition1 = dt.leftTransitions[rand.Next(dt.leftTransitions.Count)];
                        transition2 = dt2.rightTransitions[rand.Next(dt2.rightTransitions.Count)];
                        dt1 = dt2;
                        break;
                    }
                    else if (dt.right && dt2.left)
                    {
                        transition1 = dt.rightTransitions[rand.Next(dt.rightTransitions.Count)];
                        transition2 = dt2.leftTransitions[rand.Next(dt2.leftTransitions.Count)];
                        dt1 = dt2;
                        break;
                    }
                    else if (dt.top && dt2.bot)
                    {
                        transition1 = dt.topTransitions[rand.Next(dt.topTransitions.Count)];
                        transition2 = dt2.botTransitions[rand.Next(dt2.botTransitions.Count)];
                        dt1 = dt2;
                        break;
                    }
                    else if (dt.bot && dt2.top)
                    {
                        transition1 = dt.botTransitions[rand.Next(dt.botTransitions.Count)];
                        transition2 = dt2.topTransitions[rand.Next(dt2.topTransitions.Count)];
                        dt1 = dt2;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(transition1))
                {
                    transitionManager.PlaceTransitionPair(transition1, transition2);
                    dt1.Add(dt.AllTransitions);
                    dt1.Remove(transition1, transition2);
                    directed.Remove(dt);
                    i = -1;
                }
            }
            //Log("Exited BuildSpanningTree with " + directed.Count + " connected component(s).");
        }

        private static void PlaceOneWayTransitions()
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
        }

        private static void PlaceIsolatedTransitions()
        {
            if (randomizationError) return;

            List<string> isolatedTransitions = transitionManager.unplacedTransitions.Where(transition => LogicManager.GetTransitionDef(transition).isolated).ToList();
            List<string> nonisolatedTransitions = transitionManager.unplacedTransitions.Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList();
            DirectedTransitions directed = new DirectedTransitions(rand);
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
                if (failsafe > 120)
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
                if (itemManager.availableCount > 0)
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