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
        private static List<string> itemLocations;

        public static void LogHelper(string message)
        {
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "RandomizerHelperLog.txt"), message + Environment.NewLine);
        }

        public static void UpdateHelperLog()
        {
            new Thread(() =>
            {
                File.Create(Path.Combine(Application.persistentDataPath, "RandomizerHelperLog.txt")).Dispose();
                LogHelper("Generating helper log:");
                Stopwatch helperWatch = new Stopwatch();
                helperWatch.Start();

                string log = string.Empty;
                void AddToLog(string message) => log += message + Environment.NewLine;

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

                    List<string> logicTransitions = LogicManager.TransitionNames().Where(transition => LogicManager.GetTransitionDef(transition).oneWay != 2 && !LogicManager.GetTransitionDef(transition).isolated).ToList();
                    foreach (string transition in LogicManager.TransitionNames())
                    {
                        if (RandomizerMod.Instance.Settings.HasObtainedProgression(transition)) areaTransitions[LogicManager.GetTransitionDef(transition).areaName].Add(transition);
                        else if (logicTransitions.Contains(transition) && RandomizerMod.Instance.Settings.CanGetObtainedProgression(transition)) areaTransitions[LogicManager.GetTransitionDef(transition).areaName].Add("*" + transition);
                    }

                    AddToLog(Environment.NewLine + "REACHABLE TRANSITIONS");
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

                    List<string> logicTransitions = LogicManager.TransitionNames().Where(transition => LogicManager.GetTransitionDef(transition).oneWay != 2 && !LogicManager.GetTransitionDef(transition).isolated).ToList();
                    foreach (string transition in LogicManager.TransitionNames())
                    {
                        if (RandomizerMod.Instance.Settings.HasObtainedProgression(transition)) roomTransitions[LogicManager.GetTransitionDef(transition).sceneName].Add(transition);
                        else if (logicTransitions.Contains(transition) && RandomizerMod.Instance.Settings.CanGetObtainedProgression(transition)) roomTransitions[LogicManager.GetTransitionDef(transition).sceneName].Add("*" + transition);
                    }

                    AddToLog(Environment.NewLine + "REACHABLE TRANSITIONS");
                    foreach (KeyValuePair<string, List<string>> kvp in roomTransitions)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            AddToLog(Environment.NewLine + kvp.Key.Replace('_', ' ') + ":");
                            foreach (string transition in kvp.Value) AddToLog(transition);
                        }
                    }
                }

                if (itemLocations == null)
                {
                    itemLocations = new List<string>();
                    foreach (string item in LogicManager.ItemNames)
                    {
                        ReqDef def = LogicManager.GetItemDef(item);
                        if (def.isFake) continue;
                        if (def.type == ItemType.Shop) continue;

                        if (RandomizerMod.Instance.Settings.GetRandomizeByPool(def.pool))
                            itemLocations.Add(item);
                    }
                }

                Dictionary<string, List<string>> areaItemLocations = new Dictionary<string, List<string>>();
                foreach (string item in itemLocations)
                {
                    string area = LogicManager.GetItemDef(item).areaName;
                    if (!areaItemLocations.ContainsKey(area))
                    {
                        areaItemLocations[area] = new List<string>();
                    }
                }

                foreach (string location in itemLocations)
                {
                    string item = RandomizerMod.Instance.Settings.ItemPlacements.FirstOrDefault(pair => pair.Item2 == location).Item1;
                    string boolName = string.Empty;
                    if (Actions.RandomizerAction.AdditiveBoolNames.TryGetValue(item, out string _boolName)) boolName = _boolName;
                    else boolName = LogicManager.GetItemDef(item).boolName;

                    if (PlayerData.instance.GetBool(boolName)) areaItemLocations[LogicManager.GetItemDef(location).areaName].Add(location);
                    else if (RandomizerMod.Instance.Settings.GetBool(false, boolName)) areaItemLocations[LogicManager.GetItemDef(location).areaName].Add(location);
                    else if (RandomizerMod.Instance.Settings.CanGetObtainedProgression(location)) areaItemLocations[LogicManager.GetItemDef(location).areaName].Add("*" + location);
                }

                AddToLog(Environment.NewLine + "REACHABLE ITEM LOCATIONS");
                foreach (KeyValuePair<string, List<string>> kvp in areaItemLocations)
                {
                    if (kvp.Value.Count > 0)
                    {
                        AddToLog(Environment.NewLine + kvp.Key.Replace('_', ' ') + ":");
                        foreach (string item in kvp.Value) AddToLog(item.Replace('_', ' '));
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

        public static void LogItemToTrackerByBoolName(string boolName, string location)
        {
            string item = LogicManager.ItemNames.FirstOrDefault(_item => LogicManager.GetItemDef(_item).boolName == boolName);
            if (string.IsNullOrEmpty(item))
            {
                item = Actions.RandomizerAction.AdditiveBoolNames.FirstOrDefault(kvp => kvp.Value == boolName).Key;
                if (string.IsNullOrEmpty(item))
                {
                    Modding.Logger.LogWarn("Could not find item corresponding to bool: " + boolName);
                    return;
                }
            }
            LogItemToTracker(item, location);
        }

        public static void LogItemToTrackerByGeo(int geo)
        {
            string item = LogicManager.ItemNames.FirstOrDefault(_item => LogicManager.GetItemDef(_item).geo == geo);
            if (string.IsNullOrEmpty(item))
            {
                Modding.Logger.LogWarn("Could not find item corresponding to geo count: " + geo);
                return;
            }
            string location = RandomizerMod.Instance.Settings.ItemPlacements.FirstOrDefault(pair => pair.Item1 == item).Item2;
            if (string.IsNullOrEmpty(location))
            {
                Modding.Logger.LogWarn("Could not find item matched to: " + item);
                return;
            }
            LogItemToTracker(item, location);
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
            new Thread(() =>
            {
                Stopwatch spoilerWatch = new Stopwatch();
                spoilerWatch.Start();

                string log = string.Empty;
                void AddToLog(string message) => log += message + Environment.NewLine;

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

                List<string> locations = itemPlacements.Select(pair => pair.Item2).ToList();

                Dictionary<string, List<string>> areaItemLocations = new Dictionary<string, List<string>>();
                foreach (string item in locations)
                {
                    if (LogicManager.ItemNames.Contains(item))
                    {
                        string area = LogicManager.GetItemDef(item).areaName;
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
                    if (LogicManager.ItemNames.Contains(pair.Item2))
                    {
                        areaItemLocations[LogicManager.GetItemDef(pair.Item2).areaName].Add(pair.Item1 + "<---at--->" + pair.Item2);
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

                spoilerWatch.Stop();
                LogSpoiler(log);
                LogSpoiler("Generated spoiler log in " + spoilerWatch.Elapsed.TotalSeconds + " seconds.");
            }).Start();
        }
    }
}
