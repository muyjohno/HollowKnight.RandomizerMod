using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;
using RandomizerMod;
using RandomizerMod.RandomizerData;
using RandomizerMod.Extensions;
using RandomizerMod.Settings;

namespace RandomizerMod.Randomization.Logic
{
    public enum LogicToken
    {
        Operator,
        Item,
        Transition,
        Waypoint,
        Setting,
        Cost,
        Int,
    }

    // TODO: constructor
    // TODO: overrides
    public class LogicManager
    { 
        public int FlagCount => indexToItem.Count;
        public IEnumerable<string> WaypointNames => waypoints;
        public LogicMode Mode { get; private set; }

        Dictionary<string, StandardLogicDef> logicDefs = new Dictionary<string, StandardLogicDef>();
        
        // Data structures dynamically constructed to correspond to logic
        Dictionary<string, int> itemToIndex = new Dictionary<string, int>();
        List<string> indexToItem = new List<string>();
        HashSet<string> progressionItems = new HashSet<string>();
        HashSet<string> waypoints = new HashSet<string>();
        Dictionary<string, int> costNameToIndex = new Dictionary<string, int>();

        // overrides
        Dictionary<string, StandardLogicDef> overrideLogic = new Dictionary<string, StandardLogicDef>();
        Dictionary<string, Cost> overrideValues = new Dictionary<string, Cost>();

        readonly LogicProcessor LP;

        public LogicManager(LogicMode mode, RawLogicDef[] defs)
        {
            Mode = mode;
            LP = LogicProcessor.defaultProcessor;

            foreach (RawLogicDef def in defs)
            {
                logicDefs.Add(def.name, new StandardLogicDef 
                { 
                    LM = this,
                    name = def.name,
                    logic = Process(def).ToArray() 
                });
            }

            ResetOverrides();
        }

        public void AddLogicOverride(string name, StandardLogicDef def)
        {
            overrideLogic[name] = def;
        }

        public bool EvaluateLogic(string name, IProgressionManager pm)
        {
            if (overrideLogic.TryGetValue(name, out StandardLogicDef odef)) return odef.Evaluate(pm);

            if (!logicDefs.TryGetValue(name, out StandardLogicDef def))
            {
                LogWarn($"Unable to find logic for {name}.");
                return false;
            }

            return def.Evaluate(pm);
        }

        public void AddValueOverride(string item, Cost value)
        {
            overrideValues[item] = value;
        }

        public Cost? GetValue(string item)
        {
            if (overrideValues.TryGetValue(item, out Cost value)) return value;
            if (!Data.IsItem(item)) return null;

            ItemDef def = Data.GetItemDef(item);
            switch (def.action)
            {
                case GiveAction.Grub:
                case GiveAction.Int when def.fieldName == nameof(PlayerData.grubsCollected):
                    return new Cost { type = CostType.Grub, amt = def.amount > 0 ? def.amount : 1 };

                case GiveAction.Essence:
                case GiveAction.Int when def.fieldName == nameof(PlayerData.dreamOrbs):
                    return new Cost { type = CostType.Essence, amt = def.amount };

                case GiveAction.Int when def.fieldName == nameof(PlayerData.simpleKeys):
                    return new Cost { type = CostType.Simple, amt = def.amount > 0 ? def.amount : 1 };

                case GiveAction.Int when def.fieldName == nameof(PlayerData.flamesCollected):
                    return new Cost { type = CostType.Flame, amt = def.amount > 0 ? def.amount : 1 };
            }

            return null;
        }

        public int? GetIndex(string item)
        {
            if (itemToIndex.TryGetValue(item, out int index)) return index;
            return null;
        }

        public string GetItem(int id)
        {
            if (id < indexToItem.Count) return indexToItem[id];
            return null;
        }

        public void AddNewCostOverride(string logicName, Cost cost)
        {
            if (!logicDefs.TryGetValue(logicName, out var def))
            {
                LogWarn($"Attempted to override cost for nonexistent {logicName}.");
                return;
            }

            overrideLogic[logicName] = def.And(FromCost(cost));
        }

        public void ResetOverrides()
        {
            overrideLogic.Clear();
            overrideValues.Clear();

            foreach (CostDef def in Data.GetCostDefs())
            {
                if (logicDefs.TryGetValue(def.location, out var log))
                {
                    overrideLogic[def.location] = log.And(FromCost(new Cost { type = def.type, amt = def.amt }));
                }
            }
        }

        public StandardLogicDef FromCost(Cost cost)
        {
            switch (cost.type)
            {
                default:
                case CostType.None:
                    return new StandardLogicDef { LM = this, logic = new int[0] };
                case CostType.whisperingRoot:
                case CostType.Dreamnail:
                    return FromString("DREAMNAIL");
                case CostType.Wraiths:
                    return FromString("SCREAM");
                case CostType.Spore:
                    return FromString("Spore_Shroom");
                case CostType.Essence:
                case CostType.Grub:
                case CostType.Simple:
                case CostType.Flame:
                    return new StandardLogicDef { LM = this, logic = new int[] { operators["COSTOF"], (int)cost.type, cost.amt, } };
                case CostType.Geo when Mode != LogicMode.Room:
                    return FromString("GEO");
            }
        }

        public StandardLogicDef FromString(string str)
        {
            return new StandardLogicDef
            {
                logic = Process(str).ToArray(),
                LM = this,
            };
        }

        private IEnumerable<int> Process(string str)
        {
            foreach (string s in LP.Shunt(str))
            {
                switch (GetLogicToken(s))
                {
                    case LogicToken.Operator:
                        yield return operators[s];
                        continue;
                    case LogicToken.Item:
                    case LogicToken.Waypoint:
                    case LogicToken.Transition:
                    case LogicToken.Setting:
                    default:
                        if (!itemToIndex.TryGetValue(s, out int i))
                        {
                            throw new KeyNotFoundException($"Unable to process {str} into logic: token {s} was not present at LM initialization.");
                        }
                        yield return i;
                        continue;
                    case LogicToken.Cost:
                        yield return costTypes[s];
                        continue;
                    case LogicToken.Int:
                        yield return int.Parse(s);
                        continue;
                }
            }
        }

        private IEnumerable<int> Process(RawLogicDef def)
        {
            foreach (string s in LP.Shunt(def.logic))
            {
                switch (GetLogicToken(s))
                {
                    case LogicToken.Operator:
                        yield return operators[s];
                        continue;
                    case LogicToken.Item:
                        progressionItems.Add(s);
                        goto default;
                    case LogicToken.Waypoint:
                        waypoints.Add(s);
                        goto default;
                    case LogicToken.Transition:
                    case LogicToken.Setting:
                    default:
                        if (!itemToIndex.TryGetValue(s, out int i))
                        {
                            itemToIndex[s] = i = indexToItem.Count;
                            indexToItem.Add(s);
                        }
                        yield return i;
                        continue;
                    case LogicToken.Cost:
                        yield return costTypes[s];
                        continue;
                    case LogicToken.Int:
                        yield return int.Parse(s);
                        continue;
                }
            }
        }

        public static LogicToken GetLogicToken(string str)
        {
            if (operators.ContainsKey(str)) return LogicToken.Operator;
            else if (Data.IsItem(str)) return LogicToken.Item;
            else if (Data.IsTransition(str)) return LogicToken.Transition;
            else if (Data.IsLogicSetting(str)) return LogicToken.Setting;
            else if (costTypes.ContainsKey(str)) return LogicToken.Cost;
            else if (Data.IsWaypoint(str)) return LogicToken.Waypoint;
            else if (int.TryParse(str, out int _)) return LogicToken.Int;
            throw new ArgumentException($"Unable to identify token {str} found in logic");
        }

        static Dictionary<string, int> operators = new Dictionary<string, int>
        {
            { "NONE", (int)LogicOperators.NONE },
            { "ANY", (int)LogicOperators.ANY },
            { "|", (int)LogicOperators.OR },
            { "+", (int)LogicOperators.AND },
            { "$", (int)LogicOperators.COSTOF },
        };

        static Dictionary<string, int> costTypes = Enum.GetValues(typeof(CostType))
            .Cast<CostType>().ToDictionary(e => $"CostType.{e}", e => (int)e);
    }
}
