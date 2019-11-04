using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using RandomizerMod.Actions;
using static RandomizerMod.LogHelper;
using System.Text;
using static RandomizerMod.Randomization.PreRandomizer;
using static RandomizerMod.Randomization.PostRandomizer;
using static RandomizerMod.Randomization.SpanningTree;

namespace RandomizerMod.Randomization
{
    public enum RandomizerState
    {
        None,
        InProgress,
        Validating,
        Completed
    }

    internal static class Randomizer
    {
        public const int MAX_GRUB_COST = 23;
        public const int MAX_ESSENCE_COST = 900;

        public static ItemManager im;
        public static TransitionManager tm;
        public static VanillaManager vm { get { return VanillaManager.Instance; } }

        private static bool overflow;
        public static bool randomizationError;
        public static Random rand = null;

        public static List<string> startProgression;
        public static List<string> startItems;

        public static string StartName;
        public static StartDef startDef => LogicManager.GetStartLocation(StartName);        
        public static string startTransition => RandomizerMod.Instance.Settings.RandomizeRooms ? startDef.roomTransition : startDef.areaTransition;

        

        public static void Randomize()
        {
            rand = new Random(RandomizerMod.Instance.Settings.Seed);

            while (true)
            {
                randomizationError = false;
                RandomizerMod.Instance.Settings.ResetPlacements();
                RandomizeNonShopCosts();
                RandomizeStartingItems();
                RandomizeStartingLocation();
                if (RandomizerMod.Instance.Settings.RandomizeTransitions) RandomizeTransitions();
                RandomizeItems();
                if (true || !randomizationError) break;
            }

            PostRandomizationTasks();
            RandomizerAction.CreateActions(RandomizerMod.Instance.Settings.ItemPlacements, RandomizerMod.Instance.Settings);
        }

        private static void RandomizeTransitions()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();


            while (true)
            {
                Log("\n" +
                    "Beginning transition randomization...");
                SetupTransitionVariables();

                PlaceOneWayTransitions();
                if (RandomizerMod.Instance.Settings.RandomizeAreas) BuildAreaSpanningTree();
                else if (RandomizerMod.Instance.Settings.RandomizeRooms && RandomizerMod.Instance.Settings.ConnectAreas) BuildCARSpanningTree();
                else if (RandomizerMod.Instance.Settings.RandomizeRooms && !RandomizerMod.Instance.Settings.ConnectAreas) BuildRoomSpanningTree();
                else
                {
                    LogError("Ambiguous settings passed to transition randomizer.");
                    throw new NotSupportedException();
                }

                PlaceIsolatedTransitions();
                ConnectStartToGraph();
                CompleteTransitionGraph();

                if (ValidateTransitionRandomization()) break;
                if (randomizationError)
                {
                    LogWarn("Error encountered while randomizing transitions, attempting again...");
                }
            }
            watch.Stop();
            RandomizerMod.Instance.Log("Transition randomization finished in " + watch.Elapsed.TotalSeconds + " seconds.");
        }
        private static void RandomizeItems()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Log("");
            Log("Beginning item randomization...");
            SetupItemVariables();

            FirstPass();
            SecondPass();
            if (!ValidateItemRandomization())
            {
                randomizationError = true;
                return;
            }

            RandomizerMod.Instance.Log("Item randomization finished in " + watch.Elapsed.TotalSeconds + " seconds.");
        }

        private static void SetupTransitionVariables()
        {
            randomizationError = false;

            tm = new TransitionManager(rand);
            im = new ItemManager(rand);
        }
        private static void SetupItemVariables()
        {
            im = new ItemManager(rand);
            im.pm.Add(startProgression);

            overflow = false;
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

                tm.PlaceOneWayPair(horizontalEntrance, downExit);
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

                tm.PlaceOneWayPair(entrance, exit);
                oneWayEntrances.Remove(entrance);
                oneWayExits.Remove(exit);
                directed.Remove(exit);
            }
        }

        private static void PlaceIsolatedTransitions()
        {
            if (randomizationError) return;

            List<string> isolatedTransitions = tm.unplacedTransitions.Where(transition => LogicManager.GetTransitionDef(transition).isolated).ToList();
            List<string> nonisolatedTransitions = tm.unplacedTransitions.Where(transition => !LogicManager.GetTransitionDef(transition).isolated).ToList();
            DirectedTransitions directed = new DirectedTransitions(rand);
            isolatedTransitions.Remove(startTransition);
            nonisolatedTransitions.Remove(startTransition);
            directed.Add(nonisolatedTransitions);

            bool connectAreas = RandomizerMod.Instance.Settings.ConnectAreas;
            while (isolatedTransitions.Any())
            {
                string transition1 = isolatedTransitions[rand.Next(isolatedTransitions.Count)];
                string transition2 = directed.GetNextTransition(transition1, favorSameArea: connectAreas);
                if (transition2 is null)
                {
                    Log("Ran out of nonisolated transitions during preplacement!");
                    randomizationError = true;
                    return;
                }
                tm.PlaceStandbyPair(transition1, transition2);
                isolatedTransitions.Remove(transition1);
                directed.Remove(transition2);
            }
        }

        private static void ConnectStartToGraph()
        {
            if (randomizationError) return;
            Log("Attaching start to graph...");

            tm.pm = new ProgressionManager(
                RandomizerState.InProgress
                );
            im.ResetReachableLocations();
            vm.ResetReachableLocations();
            tm.ResetReachableTransitions();

            tm.pm.Add(startProgression);

            {   // keeping local variables out of the way
                DirectedTransitions d = new DirectedTransitions(rand);
                d.Add(startTransition);
                string transition2 = tm.ForceTransition(d);
                if (transition2 is null) // this should happen extremely rarely, but it has to be handled
                {
                    Log("No way out of start?!?");
                    Log("Was the start transition already placed? " + TransitionManager.transitionPlacements.ContainsKey(startTransition));
                    randomizationError = true;
                    return;
                }
                tm.PlaceTransitionPair(startTransition, transition2);
            }

            while (true)
            {
                if (im.FindNextLocation(tm.pm) != null) return;

                tm.UnloadReachableStandby();
                List<string> placeableTransitions = tm.reachableTransitions.Intersect(tm.unplacedTransitions.Union(tm.standbyTransitions.Keys)).ToList();
                if (!placeableTransitions.Any())
                {
                    Log("Could not connect start to map--ran out of placeable transitions.");
                    foreach (string t in tm.reachableTransitions) Log(t);
                    randomizationError = true;
                    return;
                }

                DirectedTransitions directed = new DirectedTransitions(rand);
                directed.Add(placeableTransitions);

                if (tm.ForceTransition(directed) is string transition1)
                {
                    string transition2 = directed.GetNextTransition(transition1);
                    tm.PlaceTransitionPair(transition1, transition2);
                }
                else
                {
                    Log("Could not connect start to map--ran out of progression transitions.");
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

            while (tm.unplacedTransitions.Any())
            {
                failsafe++;
                if (failsafe > 120)
                {
                    Log("Aborted randomization on too many passes. At the time, there were:");
                    Log("Unplaced transitions: " + tm.unplacedTransitions.Count);
                    Log("Reachable transitions: " + tm.reachableTransitions.Count);
                    Log("Reachable unplaced transitions, directionally compatible: " + tm.placeableCount);
                    Log("Reachable item locations: " + im.availableCount);
                    foreach (string t in tm.unplacedTransitions) Log(t + ", in reachable: " + tm.reachableTransitions.Contains(t) + ", is reachable: " + tm.pm.CanGet(t));
                    randomizationError = true;
                    return;
                }

                if (im.canGuess && im.availableCount > 1)
                {
                    if (im.FindNextLocation(tm.pm) is string placeLocation)
                    {
                        string placeItem = im.GuessItem();
                        im.PlaceItem(placeItem, placeLocation);
                        tm.UpdateReachableTransitions(placeItem, true);
                    }
                }

                int placeableCount = tm.placeableCount;
                if (placeableCount < 4) tm.UpdateReachableTransitions();
                if (placeableCount == 0 && im.availableCount == 0)
                {
                    Log("Ran out of locations?!?");
                    randomizationError = true;
                    return;
                }
                else if (placeableCount > 2)
                {
                    tm.UnloadReachableStandby();
                    string transition1 = tm.NextTransition();
                    string transition2 = tm.dt.GetNextTransition(transition1);
                    tm.PlaceTransitionPair(transition1, transition2);
                    continue;
                }
                else if (tm.unplacedTransitions.Count == 2)
                {
                    string transition1 = tm.unplacedTransitions[0];
                    string transition2 = tm.unplacedTransitions[1];
                    tm.PlaceTransitionPair(transition1, transition2);
                    continue;
                }
                else if (placeableCount != 0)
                {
                    if (tm.ForceTransition() is string transition1)
                    {
                        string transition2 = tm.dt.GetNextTransition(transition1);
                        tm.PlaceTransitionPair(transition1, transition2);
                        continue;
                    }
                }
                // Last ditch effort to save the seed. The list is ordered by which items are heuristically likely to unlock transitions at this point.
                if (im.FindNextLocation(tm.pm) is string lastLocation)
                {
                    foreach (string item in new List<string> { "Mantis_Claw", "Monarch_Wings", "Desolate_Dive", "Isma's_Tear", "Crystal_Heart", "Mothwing_Cloak", "Shade_Cloak" })
                    {
                        if (!tm.pm.Has(item))
                        {
                            im.PlaceItem(item, lastLocation);
                            tm.UpdateReachableTransitions(item, true);
                            break;
                        }
                    }
                    continue;
                }
            }
            Log("Placing last reserved transitions...");
            tm.UnloadStandby();
            Log("All transitions placed? " + (TransitionManager.transitionPlacements.Count == LogicManager.TransitionNames().Count(t => LogicManager.GetTransitionDef(t).oneWay != 2)));
        }

        private static void FirstPass()
        {
            Log("Beginning first pass of item placement...");

            {
                im.ResetReachableLocations();
                vm.ResetReachableLocations();
                foreach (string item in startProgression)
                {
                    im.UpdateReachableLocations(item); // overkill, but there are issues with recognizing certain starting transitions etc without this
                }
                
                Log("Finished first update");
            }

            while (true)
            {
                string placeItem;
                string placeLocation;

                switch (im.availableCount)
                {
                    case 0:
                        if (im.anyLocations)
                        {
                            if (im.canGuess)
                            {
                                if (!overflow) Log("Entered overflow state with 0 reachable locations after placing " + ItemManager.nonShopItems.Count + " locations");
                                overflow = true;
                                placeItem = im.GuessItem();
                                im.PlaceProgressionToStandby(placeItem);
                                continue;
                            }
                        }
                        return;
                    case 1:
                        placeItem = im.ForceItem();
                        if (placeItem is null)
                        {
                            if (im.canGuess)
                            {
                                if (!overflow) Log("Entered overflow state with 1 reachable location after placing " + ItemManager.nonShopItems.Count + " locations");
                                overflow = true;
                                placeItem = im.GuessItem();
                                im.PlaceProgressionToStandby(placeItem);
                                continue;
                            }
                            else placeItem = im.NextItem();
                        }
                        else
                        {
                            im.Delinearize(rand);
                        }
                        placeLocation = im.NextLocation();
                        break;
                    default:
                        placeItem = im.NextItem();
                        placeLocation = im.NextLocation();
                        break;
                }

                //Log($"i: {placeItem}, l: {placeLocation}, o: {overflow}, p: {LogicManager.GetItemDef(placeItem).progression}");

                if (!overflow && !LogicManager.GetItemDef(placeItem).progression)
                {
                    im.PlaceJunkItemToStandby(placeItem, placeLocation);
                }
                else
                {
                    im.PlaceItem(placeItem, placeLocation);
                }
            }
        }

        private static void SecondPass()
        {
            Log("Beginning second pass of item placement...");
            im.TransferStandby();

            // We fill the remaining locations and shops with the leftover junk
            while (im.anyItems)
            {
                string placeItem = im.NextItem(checkFlag: false);
                string placeLocation;

                if (im.anyLocations) placeLocation = im.NextLocation(checkLogic: false);
                else placeLocation = LogicManager.ShopNames[rand.Next(5)];

                im.PlaceItemFromStandby(placeItem, placeLocation);
            }
        }

        private static bool ValidateTransitionRandomization()
        {
            if (randomizationError) return false;
            Log("Beginning transition placement validation...");

            ProgressionManager pm = new ProgressionManager(
                RandomizerState.Validating
                );
            pm.Add(startProgression);
            pm.Add(LogicManager.ItemNames.Where(i => LogicManager.GetItemDef(i).progression));

            tm.ResetReachableTransitions();
            tm.UpdateReachableTransitions(_pm: pm);
            
            bool validated = tm.reachableTransitions.SetEquals(LogicManager.TransitionNames());

            if (!validated)
            {
                Log("Transition placements failed to validate!");
                foreach (string t in LogicManager.TransitionNames().Except(tm.reachableTransitions)) Log(t);
            }
            else Log("Validation successful.");
            return validated;
        }

        private static bool ValidateItemRandomization()
        {
            RandomizerMod.Instance.Log("Beginning item placement validation...");

            if (im.randomizedLocations.Except(ItemManager.nonShopItems.Keys).Except(ItemManager.shopItems.Keys).Any())
            {
                Log("Unable to validate!");
                string m = "The following locations were not filled: ";
                foreach (string l in im.randomizedLocations.Except(ItemManager.nonShopItems.Keys).Except(ItemManager.shopItems.Keys)) m += l + ", ";
                Log(m);
                return false;
            }

            ProgressionManager pm = new ProgressionManager(
                RandomizerState.Validating
                );
            pm.Add(startProgression);

            HashSet<string> everything = new HashSet<string>(im.randomizedLocations.Union(vm.progressionLocations));

            if (RandomizerMod.Instance.Settings.RandomizeTransitions)
            {
                everything.UnionWith(LogicManager.TransitionNames());
                tm.ResetReachableTransitions();
                tm.UpdateReachableTransitions(_pm: pm);
            }

            vm.ResetReachableLocations(false, pm);

            int passes = 0;
            while (everything.Any())
            {
                if (RandomizerMod.Instance.Settings.RandomizeTransitions) everything.ExceptWith(tm.reachableTransitions);

                foreach (string location in im.randomizedLocations.Union(vm.progressionLocations).Where(loc => everything.Contains(loc) && pm.CanGet(loc)))
                {
                    everything.Remove(location);
                    if (LogicManager.ShopNames.Contains(location))
                    {
                        if (ItemManager.shopItems.Keys.Contains(location))
                        {
                            foreach (string newItem in ItemManager.shopItems[location])
                            {
                                if (LogicManager.GetItemDef(newItem).progression)
                                {
                                    pm.Add(newItem);
                                    if (RandomizerMod.Instance.Settings.RandomizeTransitions) tm.UpdateReachableTransitions(newItem, true, pm);
                                }
                            }
                        }

                        if (vm.progressionLocations.Contains(location))
                        {
                            vm.UpdateVanillaLocations(location, false, pm);
                        }
                    }
                    else if (vm.progressionLocations.Contains(location))
                    {
                        vm.UpdateVanillaLocations(location, false, pm);
                    }
                    else if (LogicManager.GetItemDef(ItemManager.nonShopItems[location]).progression)
                    {
                        pm.Add(ItemManager.nonShopItems[location]);
                        if (RandomizerMod.Instance.Settings.RandomizeTransitions) tm.UpdateReachableTransitions(ItemManager.nonShopItems[location], true, pm);
                    }
                }

                passes++;
                if (passes > 400)
                {
                    Log("Unable to validate!");
                    Log("Able to get items: " + pm.ListObtainedProgression() + Environment.NewLine + "Grubs: " + pm.obtained[LogicManager.grubIndex] + Environment.NewLine + "Essence: " + pm.obtained[LogicManager.essenceIndex]);
                    string m = string.Empty;
                    foreach (string s in everything) m += s + ", ";
                    Log("Unable to get locations: " + m);
                    LogItemPlacements(pm);
                    return false;
                }
            }
            //LogItemPlacements(pm);
            Log("Validation successful.");
            return true;
        }
    }
}
