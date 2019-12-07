using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using Modding;
using RandomizerMod.Randomization;

namespace RandomizerMod
{
    public static class RandoLogger
    {
        private static ProgressionManager pm;
        private static HashSet<string> obtainedLocations;
        private static HashSet<string> uncheckedLocations;
        private static HashSet<string> obtainedTransitions;
        private static HashSet<string> uncheckedTransitions;
        private static int seed;

        private static void UpdateItemLists(bool forceUpdate = false)
        {
            if (true || obtainedLocations is null || uncheckedLocations is null || pm is null || seed != RandomizerMod.Instance.Settings.Seed || forceUpdate)
            {
                obtainedLocations = new HashSet<string>();
                uncheckedLocations = new HashSet<string>();
                pm = new ProgressionManager(
                    RandomizerState.Completed
                    );
                seed = RandomizerMod.Instance.Settings.Seed;

                foreach (string item in ItemManager.GetRandomizedItems())
                {
                    if (RandomizerMod.Instance.Settings.GetBool(false, item))
                    {
                        obtainedLocations.Add(RandomizerMod.Instance.Settings.ItemPlacements.FirstOrDefault(pair => pair.Item1 == item).Item2);
                        if (LogicManager.GetItemDef(item).progression)
                        {
                            pm.Add(item);
                        }
                    }
                }

                if (RandomizerMod.Instance.Settings.RandomizeTransitions)
                {
                    obtainedTransitions = new HashSet<string>();
                    uncheckedTransitions = new HashSet<string>();

                    foreach (string transition in LogicManager.TransitionNames())
                    {
                        if (RandomizerMod.Instance.Settings.GetBool(false, transition))
                        {
                            obtainedTransitions.Add(transition);
                            pm.Add(transition);
                        }
                    }
                }
            }

            foreach (string location in ItemManager.GetRandomizedLocations())
            {
                string altLocation = location; // clumsy way to be able to switch out items without spoiling their costs

                if (obtainedLocations.Contains(location)) continue;

                if (!LogicManager.ShopNames.Contains(location))
                {
                    if (LogicManager.GetItemDef(location).costType == Actions.AddYNDialogueToShiny.CostType.Essence)
                    {
                        altLocation = "Seer";
                    }
                    else if (LogicManager.GetItemDef(location).costType == Actions.AddYNDialogueToShiny.CostType.Grub)
                    {
                        altLocation = "Grubfather";
                    }
                }

                if (pm.CanGet(altLocation))
                {
                    if (!RandomizerMod.Instance.Settings.GetBool(false, RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item2 == location).Item1))
                    {
                        uncheckedLocations.Add(altLocation);
                    }
                }
            }

            if (!RandomizerMod.Instance.Settings.RandomizeTransitions) return;

            foreach (string transition in LogicManager.TransitionNames())
            {
                if (obtainedTransitions.Contains(transition))
                {
                    continue;
                }
                if (pm.Has(transition))
                {
                    obtainedTransitions.Add(transition);
                }
                else if (pm.CanGet(transition))
                {
                    uncheckedTransitions.Add(transition);
                }
            }
        }

        public static void LogHelper(string message)
        {
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "RandomizerHelperLog.txt"), message + Environment.NewLine);
        }

        public static void UpdateHelperLog(string newThing = "Launch", bool gotItem = false, bool gotTransition = false, bool forceUpdate = false)
        {
            new Thread(() =>
            {
                File.Create(Path.Combine(Application.persistentDataPath, "RandomizerHelperLog.txt")).Dispose();
                LogHelper("Generating helper log in response to: " + newThing.Replace('_', ' '));
                Stopwatch helperWatch = new Stopwatch();
                helperWatch.Start();

                string log = string.Empty;
                void AddToLog(string message) => log += message + Environment.NewLine;


                if (pm == null || forceUpdate)
                {
                    UpdateItemLists(forceUpdate);
                }
                else if (gotItem)
                {
                    string loc = RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == newThing).Item2;
                    obtainedLocations.Add(loc);
                    uncheckedLocations.Remove(loc);

                    if (LogicManager.GetItemDef(newThing).progression)
                    {
                        pm.Add(newThing);
                    }

                    UpdateItemLists();
                }
                else if (gotTransition)
                {
                    // Should always pass entrance, so we can pull out the destination from the dictionary
                    RandomizerMod.Instance.Settings._transitionPlacements.TryGetValue(newThing, out string t2);
                    pm.Add(newThing);
                    obtainedTransitions.Add(newThing);
                    uncheckedTransitions.Remove(newThing);
                    pm.Add(t2);
                    obtainedTransitions.Add(t2);
                    uncheckedTransitions.Remove(t2);
                    UpdateItemLists();
                }

                AddToLog(Environment.NewLine + "Reachable grubs: " + pm.obtained[LogicManager.grubIndex]);
                AddToLog("Reachable essence: " + pm.obtained[LogicManager.essenceIndex]);

                // UNCHECKED ITEMS
                {
                    AddToLog(Environment.NewLine + Environment.NewLine + "REACHABLE ITEM LOCATIONS");

                    Dictionary<string, List<string>> AreaSortedItems = new Dictionary<string, List<string>>();
                    List<string> shops = LogicManager.ShopNames.Union(new List<string> { "Seer", "Grubfather" }).ToList();

                    foreach (string location in uncheckedLocations)
                    {
                        if (shops.Contains(location))
                        {
                            if (!AreaSortedItems.ContainsKey("Shops"))
                            {
                                AreaSortedItems.Add("Shops", new List<string>());
                            }
                            AreaSortedItems["Shops"].Add(location);
                            continue;
                        }

                        if (AreaSortedItems.ContainsKey(LogicManager.GetItemDef(location).areaName)) continue;

                        AreaSortedItems.Add(
                            LogicManager.GetItemDef(location).areaName,
                            uncheckedLocations.Where(loc => !shops.Contains(loc) && LogicManager.GetItemDef(loc).areaName == LogicManager.GetItemDef(location).areaName).ToList()
                            );
                    }

                    foreach (var area in AreaSortedItems)
                    {
                        AddToLog(Environment.NewLine + area.Key.Replace('_', ' '));
                        foreach (string location in area.Value)
                        {
                            AddToLog(" - " + location.Replace('_', ' '));
                        }
                    }
                }

                // UNCHECKED TRANSITIONS (AREA RANDOMIZER VERSION)
                if (RandomizerMod.Instance.Settings.RandomizeAreas)
                {
                    AddToLog(Environment.NewLine + Environment.NewLine + "REACHABLE TRANSITIONS");

                    Dictionary<string, List<string>> AreaSortedTransitions = new Dictionary<string, List<string>>();
                    foreach (string transition in uncheckedTransitions)
                    {
                        if (AreaSortedTransitions.ContainsKey(LogicManager.GetTransitionDef(transition).areaName)) continue;

                        AreaSortedTransitions.Add(
                            LogicManager.GetTransitionDef(transition).areaName,
                            uncheckedTransitions.Where(t => LogicManager.GetTransitionDef(t).areaName == LogicManager.GetTransitionDef(transition).areaName).ToList()
                            );
                    }

                    foreach (var area in AreaSortedTransitions)
                    {
                        AddToLog(Environment.NewLine + area.Key.Replace('_', ' '));
                        foreach (string transition in area.Value)
                        {
                            AddToLog(" - " + transition);
                        }
                    }
                }
                else if (RandomizerMod.Instance.Settings.RandomizeRooms)
                {
                    AddToLog(Environment.NewLine + Environment.NewLine + "REACHABLE TRANSITIONS");

                    Dictionary<string, List<string>> SceneSortedTransitions = new Dictionary<string, List<string>>();
                    foreach (string transition in uncheckedTransitions)
                    {
                        if (SceneSortedTransitions.ContainsKey(LogicManager.GetTransitionDef(transition).sceneName.Split('-').First())) continue;

                        SceneSortedTransitions.Add(
                            LogicManager.GetTransitionDef(transition).sceneName.Split('-').First(),
                            uncheckedTransitions.Where
                                (t => LogicManager.GetTransitionDef(t).sceneName.Split('-').First()
                                    == LogicManager.GetTransitionDef(transition).sceneName.Split('-').First()).ToList()
                            );
                    }

                    foreach (var room in SceneSortedTransitions)
                    {
                        AddToLog(Environment.NewLine + room.Key.Replace('_', ' '));
                        foreach (string transition in room.Value)
                        {
                            AddToLog(" - " + transition);
                        }
                    }
                }

                helperWatch.Stop();
                LogHelper(log);
                LogHelper("Generated helper log in " + helperWatch.Elapsed.TotalSeconds + " seconds.");
            }).Start();
        }

        public static void LogTracker(string message)
        {
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "RandomizerTrackerLog.txt"), message + Environment.NewLine);
        }
        public static void InitializeTracker()
        {
            File.Create(Path.Combine(Application.persistentDataPath, "RandomizerTrackerLog.txt")).Dispose();
            LogTracker("Beginning new playthrough with seed: " + RandomizerMod.Instance.Settings.Seed);
        }
        public static void LogTransitionToTracker(string entrance, string exit)
        {
            string message = string.Empty;
            if (RandomizerMod.Instance.Settings.RandomizeAreas)
            {
                string area1 = LogicManager.GetTransitionDef(entrance).areaName.Replace('_', ' ');
                string area2 = LogicManager.GetTransitionDef(exit).areaName.Replace('_', ' ');
                message = "TRANSITION --- " + area1 + " --> " + area2 + " (" + entrance + " --> " + exit + ")";
            }
            else if (RandomizerMod.Instance.Settings.RandomizeRooms)
            {
                message = "TRANSITION --- " + entrance + " --> " + exit;
            }

            LogTracker(message);
        }
        public static void LogItemToTracker(string item, string location)
        {
            item = item.Split('-').First();

            string message = "ITEM --- " + item.Replace('_', ' ') + " at " + location.Replace('_', ' ');
            LogTracker(message);
        }

        public static void LogHintToTracker(string hint, bool jiji = true, bool quirrel = false)
        {
            if (jiji) LogTracker("HINT " + RandomizerMod.Instance.Settings.JijiHintCounter + " --- " + hint);
            else if (quirrel) LogTracker("HINT (QUIRREL) --- " + hint);
            else LogTracker("HINT --- " + hint);
        }

        public static void LogSpoiler(string message)
        {
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "RandomizerSpoilerLog.txt"), message + Environment.NewLine);
        }

        public static void InitializeSpoiler()
        {
            File.Create(Path.Combine(Application.persistentDataPath, "RandomizerSpoilerLog.txt")).Dispose();
            LogSpoiler("Randomization completed with seed: " + RandomizerMod.Instance.Settings.Seed);
        }

        public static void LogTransitionToSpoiler(string entrance, string exit)
        {
            string message = "Entrance " + entrance + " linked to exit " + exit;
            LogSpoiler(message);
        }

        public static void LogItemToSpoiler(string item, string location)
        {
            string message = $"Putting item \"{item.Replace('_', ' ')}\" at \"{location.Replace('_', ' ')}\"";
            LogSpoiler(message);
        }
        public static void LogAllToSpoiler((string, string)[] itemPlacements, (string, string)[] transitionPlacements)
        {
            RandomizerMod.Instance.Log("Generating spoiler log...");
            new Thread(() =>
            {
                Stopwatch spoilerWatch = new Stopwatch();
                spoilerWatch.Start();

                string log = string.Empty;
                void AddToLog(string message) => log += message + Environment.NewLine;

                AddToLog(GetItemSpoiler(itemPlacements));
                AddToLog(GetTransitionSpoiler(transitionPlacements));

                try 
                {
                    AddToLog(Environment.NewLine + "SETTINGS");
                    AddToLog($"Seed: {RandomizerMod.Instance.Settings.Seed}");
                    AddToLog($"Mode: " + // :)
                        $"{(RandomizerMod.Instance.Settings.RandomizeRooms ? (RandomizerMod.Instance.Settings.ConnectAreas ? "Connected-Area Room Randomizer" : "Room Randomizer") : (RandomizerMod.Instance.Settings.RandomizeAreas ? "Area Randomizer" : "Item Randomizer"))}");
                    AddToLog($"Cursed: {RandomizerMod.Instance.Settings.Cursed}");
                    AddToLog($"Start location: {RandomizerMod.Instance.Settings.StartName}");
                    AddToLog($"Random start items: {RandomizerMod.Instance.Settings.RandomizeStartItems}");
                    AddToLog("REQUIRED SKIPS");
                    AddToLog($"Mild skips: {RandomizerMod.Instance.Settings.MildSkips}");
                    AddToLog($"Shade skips: {RandomizerMod.Instance.Settings.ShadeSkips}");
                    AddToLog($"Fireball skips: {RandomizerMod.Instance.Settings.FireballSkips}");
                    AddToLog($"Acid skips: {RandomizerMod.Instance.Settings.AcidSkips}");
                    AddToLog($"Spike tunnels: {RandomizerMod.Instance.Settings.SpikeTunnels}");
                    AddToLog($"Dark Rooms: {RandomizerMod.Instance.Settings.DarkRooms}");
                    AddToLog($"Spicy skips: {RandomizerMod.Instance.Settings.SpicySkips}");
                    AddToLog("RANDOMIZED LOCATIONS");
                    AddToLog($"Dreamers: {RandomizerMod.Instance.Settings.RandomizeDreamers}");
                    AddToLog($"Skills: {RandomizerMod.Instance.Settings.RandomizeSkills}");
                    AddToLog($"Charms: {RandomizerMod.Instance.Settings.RandomizeCharms}");
                    AddToLog($"Keys: {RandomizerMod.Instance.Settings.RandomizeKeys}");
                    AddToLog($"Geo chests: {RandomizerMod.Instance.Settings.RandomizeGeoChests}");
                    AddToLog($"Mask shards: {RandomizerMod.Instance.Settings.RandomizeMaskShards}");
                    AddToLog($"Vessel fragments: {RandomizerMod.Instance.Settings.RandomizeVesselFragments}");
                    AddToLog($"Pale ore: {RandomizerMod.Instance.Settings.RandomizePaleOre}");
                    AddToLog($"Charm notches: {RandomizerMod.Instance.Settings.RandomizeCharmNotches}");
                    AddToLog($"Rancid eggs: {RandomizerMod.Instance.Settings.RandomizeRancidEggs}");
                    AddToLog($"Relics: {RandomizerMod.Instance.Settings.RandomizeRelics}");
                    AddToLog($"Stags: {RandomizerMod.Instance.Settings.RandomizeStags}");
                    AddToLog($"Maps: {RandomizerMod.Instance.Settings.RandomizeMaps}");
                    AddToLog($"Grubs: {RandomizerMod.Instance.Settings.RandomizeGrubs}");
                    AddToLog($"Whispering roots: {RandomizerMod.Instance.Settings.RandomizeWhisperingRoots}");
                    AddToLog("QUALITY OF LIFE");
                    AddToLog($"Lemm: {RandomizerMod.Instance.Settings.Lemm}");
                    AddToLog($"Salubra: {RandomizerMod.Instance.Settings.CharmNotch}");
                    AddToLog($"Early geo: {RandomizerMod.Instance.Settings.EarlyGeo}");
                    AddToLog($"Jiji: {RandomizerMod.Instance.Settings.Jiji}");
                    AddToLog($"Quirrel: {RandomizerMod.Instance.Settings.Quirrel}");
                    AddToLog($"Levers: {RandomizerMod.Instance.Settings.LeverSkips}");
                }
                catch
                {
                    AddToLog("Error logging randomizer settings!?!?");
                }

                spoilerWatch.Stop();
                LogSpoiler(log);
                LogSpoiler("Generated spoiler log in " + spoilerWatch.Elapsed.TotalSeconds + " seconds.");
            }).Start();
        }

        private static string GetTransitionSpoiler((string, string)[] transitionPlacements)
        {
            string log = string.Empty;
            void AddToLog(string message) => log += message + Environment.NewLine;

            try
            {
                if (RandomizerMod.Instance.Settings.RandomizeAreas)
                {
                    Dictionary<string, List<string>> areaTransitions = new Dictionary<string, List<string>>();
                    foreach (string transition in LogicManager.TransitionNames())
                    {
                        string area = LogicManager.GetTransitionDef(transition).areaName;
                        if (!areaTransitions.ContainsKey(area))
                        {
                            areaTransitions[area] = new List<string>();
                        }
                    }

                    foreach ((string, string) pair in transitionPlacements)
                    {
                        string area = LogicManager.GetTransitionDef(pair.Item1).areaName;
                        areaTransitions[area].Add(pair.Item1 + " --> " + pair.Item2);
                    }

                    AddToLog(Environment.NewLine + "TRANSITIONS");
                    foreach (KeyValuePair<string, List<string>> kvp in areaTransitions)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            AddToLog(Environment.NewLine + kvp.Key.Replace('_', ' ') + ":");
                            foreach (string transition in kvp.Value) AddToLog(transition);
                        }
                    }
                }

                if (RandomizerMod.Instance.Settings.RandomizeRooms)
                {
                    Dictionary<string, List<string>> roomTransitions = new Dictionary<string, List<string>>();
                    foreach (string transition in LogicManager.TransitionNames())
                    {
                        string room = LogicManager.GetTransitionDef(transition).sceneName;
                        if (!roomTransitions.ContainsKey(room))
                        {
                            roomTransitions[room] = new List<string>();
                        }
                    }

                    foreach ((string, string) pair in transitionPlacements)
                    {
                        string room = LogicManager.GetTransitionDef(pair.Item1).sceneName;
                        roomTransitions[room].Add(pair.Item1 + " --> " + pair.Item2);
                    }

                    AddToLog(Environment.NewLine + "TRANSITIONS");
                    foreach (KeyValuePair<string, List<string>> kvp in roomTransitions)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            AddToLog(Environment.NewLine + kvp.Key.Replace('_', ' ') + ":");
                            foreach (string transition in kvp.Value) AddToLog(transition);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RandomizerMod.Instance.LogError("Error while creating transition spoiler log: " + e);
            }
            return log;
        }

        private static string GetItemSpoiler((string, string)[] itemPlacements)
        {
            string log = string.Empty;
            void AddToLog(string message) => log += message + Environment.NewLine;
            try
            {
                List<string> locations = itemPlacements.Select(pair => pair.Item2).ToList();

                Dictionary<string, List<string>> areaItemLocations = new Dictionary<string, List<string>>();
                foreach (string item in locations)
                {
                    if (LogicManager.TryGetItemDef(item, out ReqDef locationDef))
                    {
                        string area = locationDef.areaName;
                        if (!areaItemLocations.ContainsKey(area))
                        {
                            areaItemLocations[area] = new List<string>();
                        }
                    }
                    else if (!areaItemLocations.ContainsKey(item))
                    {
                        areaItemLocations[item] = new List<string>();
                    }
                }

                List<string> progression = new List<string>();
                foreach ((string, string) pair in itemPlacements)
                {
                    if (LogicManager.GetItemDef(pair.Item1).progression) progression.Add(pair.Item1 + "<---at--->" + pair.Item2);
                    if (LogicManager.TryGetItemDef(pair.Item2, out ReqDef locationDef))
                    {
                        areaItemLocations[locationDef.areaName].Add(pair.Item1 + "<---at--->" + pair.Item2);
                    }
                    else areaItemLocations[pair.Item2].Add(pair.Item1);
                }

                AddToLog(Environment.NewLine + "PROGRESSION ITEMS");
                foreach (string item in progression) AddToLog(item.Replace('_', ' '));

                AddToLog(Environment.NewLine + "ALL ITEMS");
                foreach (KeyValuePair<string, List<string>> kvp in areaItemLocations)
                {
                    if (kvp.Value.Count > 0)
                    {
                        AddToLog(Environment.NewLine + kvp.Key.Replace('_', ' ') + ":");
                        foreach (string item in kvp.Value) AddToLog(item.Replace('_', ' '));
                    }
                }
            }
            catch (Exception e)
            {
                RandomizerMod.Instance.LogError("Error while creating item spoiler log: " + e);
            }
            return log;
        }
    }
}
