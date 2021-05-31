using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static RandomizerMod.LogHelper;
using RandomizerMod.Randomization.Logic;

namespace RandomizerMod.RandomizerData
{
    public static class Data
    {
        // Items
        private static string[] _itemNames;
        private static Dictionary<string, ItemDef> _items;
        private static Dictionary<string, string[]> _pooledItemNames;
        private static Dictionary<string, ItemDef[]> _pooledItems;

        // Locations
        private static string[] _locationNames;
        private static Dictionary<string, LocationDef> _locations;
        private static Dictionary<string, string[]> _pooledLocationNames;
        private static Dictionary<string, LocationDef[]> _pooledLocations;

        // Transitions
        private static Dictionary<string, TransitionDef> _areaTransitions;
        private static Dictionary<string, TransitionDef> _roomTransitions;

        // Starts
        private static string[] _startNames;
        private static Dictionary<string, StartDef> _starts;

        // Logic
        private static RawLogicDef[] _rawItemRandomizerLogic;
        private static RawLogicDef[] _rawAreaRandomizerLogic;
        private static RawLogicDef[] _rawRoomRandomizerLogic;
        private static HashSet<string> _waypointNames;

        // Costs
        private static CostDef[] _costs;

        // Logic Settings
        private static Dictionary<string, LogicSettingDef> _logicSettings; // name in logic --> settings path

        // Pools
        private static Dictionary<string, string> _pools; // pool name --> settings path

        // Vanilla
        private static Dictionary<string, VanillaDef[]> _pooledVanillaDefs;

        #region Item Methods

        public static ItemDef GetItemDef(string name)
        {
            if (_items.TryGetValue(name, out var def)) return def;
            
            LogWarn($"Unable to find ItemDef for {name}.");
            return null;
        }

        public static string[] GetItemNames()
        {
            return _itemNames.ToArray();
        }

        public static ItemDef[] GetItemArray()
        {
            return _items.Values.ToArray();
        }

        public static string[] GetItemNamesByPool(string pool)
        {
            if (_pooledItemNames.TryGetValue(pool, out var defs)) return defs.ToArray();

            LogWarn($"Unable to find item names for {pool}.");
            return null;
        }

        public static ItemDef[] GetItemsByPool(string pool)
        {
            if (_pooledItems.TryGetValue(pool, out var defs)) return defs.ToArray();

            LogWarn($"Unable to find ItemDefs for {pool}.");
            return null;
        }

        public static bool IsItem(string item)
        {
            return _items.ContainsKey(item);
        }

        #endregion
        #region Location Methods

        public static LocationDef GetLocationDef(string name)
        {
            if (_locations.TryGetValue(name, out var def)) return def;

            LogWarn($"Unable to find LocationDef for {name}.");
            return null;
        }

        public static string[] GetLocationNames()
        {
            return _locationNames.ToArray();
        }

        public static LocationDef[] GetLocationArray()
        {
            return _locations.Values.ToArray();
        }

        public static string[] GetLocationNamesByPool(string pool)
        {
            if (_pooledLocationNames.TryGetValue(pool, out var names)) return names.ToArray();

            LogWarn($"Unable to find location names for {pool}.");
            return null;
        }

        public static LocationDef[] GetLocationsByPool(string pool)
        {
            if (_pooledLocations.TryGetValue(pool, out var defs)) return defs.ToArray();

            LogWarn($"Unable to find LocationDefs for {pool}.");
            return null;
        }

        public static bool IsLocation(string location)
        {
            return _locations.ContainsKey(location);
        }

        #endregion
        #region Transition Methods
        public static TransitionDef GetTransitionDef(string name)
        {
            if (_roomTransitions.TryGetValue(name, out TransitionDef def)) return def;

            LogWarn($"Unable to find TransitionDef for {name}.");
            return null;
        }

        public static IEnumerable<string> GetAreaTransitionNames()
        {
            return _areaTransitions.Keys;
        }

        public static IEnumerable<string> GetRoomTransitionNames()
        {
            return _areaTransitions.Keys;
        }

        public static bool IsAreaTransition(string str)
        {
            return _areaTransitions.ContainsKey(str);
        }

        public static bool IsTransition(string str)
        {
            return _roomTransitions.ContainsKey(str);
        }

        public static bool IsTransitionWithEntry(string str)
        {
            return _roomTransitions.TryGetValue(str, out var def) && def.oneWay != 2;
        }

        public static bool IsExitOnlyTransition(string str)
        {
            return _roomTransitions.TryGetValue(str, out var def) && def.oneWay == 2;
        }

        public static bool IsEnterOnlyTransition(string str)
        {
            return _roomTransitions.TryGetValue(str, out var def) && def.oneWay == 1;
        }
        #endregion
        #region Start Methods

        public static bool IsStart(string str)
        {
            return _starts.ContainsKey(str);
        }

        public static StartDef GetStartDef(string str)
        {
            if (_starts.TryGetValue(str, out var def)) return def;

            LogWarn($"Unable to find StartDef for {str}.");
            return null;
        }

        public static IEnumerable<string> GetStartNames()
        {
            return _startNames;
        }

        #endregion
        #region Logic Methods

        public static RawLogicDef[] GetRawLogicDefsByMode(LogicMode mode)
        {
            switch (mode)
            {
                default:
                case LogicMode.Item:
                    return _rawItemRandomizerLogic.ToArray();
                case LogicMode.Area:
                    return _rawAreaRandomizerLogic.ToArray();
                case LogicMode.Room:
                    return _rawRoomRandomizerLogic.ToArray();
            }
        }

        public static bool IsWaypoint(string str)
        {
            return _waypointNames.Contains(str);
        }

        #endregion
        #region Cost Methods

        public static IEnumerable<CostDef> GetCostDefs()
        {
            return _costs;
        }

        #endregion
        #region Logic Settings Methods

        public static bool IsLogicSetting(string str)
        {
            return _logicSettings.ContainsKey(str);
        }

        public static string[] GetLogicNames()
        {
            return _logicSettings.Keys.ToArray();
        }

        public static IEnumerable<string> GetApplicableLogicSettings(Settings.GenerationSettings settings)
        {
            foreach (var kvp in _logicSettings) 
                if (Settings.Util.Get(settings, kvp.Value.path) is bool value && (kvp.Value.negate ? !value : value))
                    yield return kvp.Key;
        }

        #endregion
        #region Pool Methods

        public static bool IsPool(string str)
        {
            return _pools.ContainsKey(str);
        }

        public static string[] GetPoolNames()
        {
            return _pools.Keys.ToArray();
        }

        public static IEnumerable<string> GetApplicablePools(Settings.GenerationSettings settings)
        {
            foreach (var kvp in _pools) if (Settings.Util.Get(settings, kvp.Value) is bool value && value) yield return kvp.Key;
        }

        #endregion
        #region Vanilla Methods

        public static VanillaDef[] GetVanillaDefsByPool(string pool)
        {
            if (_pooledVanillaDefs.TryGetValue(pool, out var defs)) return defs;

            return new VanillaDef[0];
        }

        public static IEnumerable<VanillaDef> GetApplicableVanillaDefs(Settings.GenerationSettings settings)
        {
            return _pools.Where(kvp => _pooledVanillaDefs.ContainsKey(kvp.Key) && Settings.Util.Get(settings, kvp.Value) is bool value && !value)
                .SelectMany(kvp => _pooledVanillaDefs[kvp.Key]);
        }


        #endregion


        public static void Setup()
        {
            // Load XMLs and set up queries
            IEnumerable<XmlNode> items = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.items.xml")
                .SelectNodes("randomizer/item").Cast<XmlNode>();
            IEnumerable<XmlNode> locations = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.locations.xml")
                .SelectNodes("randomizer/item").Cast<XmlNode>();
            IEnumerable<XmlNode> areas = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.areas.xml")
                .SelectNodes("randomizer/transition").Cast<XmlNode>();
            IEnumerable<XmlNode> rooms = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.rooms.xml")
                .SelectNodes("randomizer/transition").Cast<XmlNode>();
            IEnumerable<XmlNode> itemLogic = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.itemlogic.xml")
                .SelectNodes("randomizer/item").Cast<XmlNode>();
            IEnumerable<XmlNode> macros = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.macros.xml")
                .SelectNodes("randomizer/macro").Cast<XmlNode>();
            IEnumerable<XmlNode> startLocations = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.startlocations.xml")
                .SelectNodes("randomizer/start").Cast<XmlNode>();
            IEnumerable<XmlNode> waypoints = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.waypoints.xml")
                .SelectNodes("randomizer/item").Cast<XmlNode>();
            IEnumerable<XmlNode> costs = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.costs.xml")
                .SelectNodes("randomizer/cost").Cast<XmlNode>();
            IEnumerable<XmlNode> logic_settings = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.logic_settings.xml")
                .SelectNodes("randomizer/item").Cast<XmlNode>();
            IEnumerable<XmlNode> pools = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.pools.xml")
                .SelectNodes("randomizer/item").Cast<XmlNode>();
            IEnumerable<XmlNode> vanilla = XmlUtil.LoadEmbeddedXml("RandomizerMod.Resources.vanilla.xml")
                .SelectNodes("randomizer/ilp").Cast<XmlNode>();

            // ItemDefs
            _itemNames = items.Select(i => i.GetNameAttribute()).ToArray();
            _items = items.ToDictionary(node => XmlUtil.GetNameAttribute(node), node => XmlUtil.DeserializeByReflection<ItemDef>(node));
            _pooledItemNames = _itemNames.GroupBy(i => _items[i].pool)
                .ToDictionary(i => i.Key, i => i
                .SelectMany(s => Enumerable.Repeat(s, Math.Max(_items[s].count, 1))).ToArray());
            _pooledItems = _items.Values.GroupBy(i => i.pool).ToDictionary(g => g.Key, g => g.ToArray());


            // LocationDefs
            _locationNames = locations.Select(l => l.GetNameAttribute()).ToArray();
            _locations = locations.ToDictionary(node => XmlUtil.GetNameAttribute(node), node => XmlUtil.DeserializeByReflection<LocationDef>(node));
            _pooledLocationNames = _locationNames.GroupBy(l => _locations[l].pool).ToDictionary(g => g.Key, g => g.ToArray());
            _pooledLocations = _locations.Values.GroupBy(l => l.pool).ToDictionary(g => g.Key, g => g.ToArray());

            // TransitionDefs
            _areaTransitions = areas.ToDictionary(node => XmlUtil.GetNameAttribute(node), node => XmlUtil.DeserializeByReflection<TransitionDef>(node));
            _roomTransitions = rooms.ToDictionary(node => XmlUtil.GetNameAttribute(node), node => XmlUtil.DeserializeByReflection<TransitionDef>(node));

            // StartDefs
            _startNames = startLocations.Select(node => node.GetNameAttribute()).ToArray();
            _starts = startLocations.ToDictionary(node => node.GetNameAttribute(), node => node.DeserializeByReflection<StartDef>());

            // Logic
            ModeLogicDef[] locLogic = itemLogic.Select(node => XmlUtil.DeserializeByReflection<ModeLogicDef>(node)).ToArray();
            ModeLogicDef[] waypointLogic = waypoints.Select(node => XmlUtil.DeserializeByReflection<ModeLogicDef>(node)).ToArray();
            RawLogicDef[] areaTransitionLogic = areas.Select(node => XmlUtil.DeserializeByReflection<RawLogicDef>(node)).ToArray();
            RawLogicDef[] roomTransitionLogic = rooms.Select(node => XmlUtil.DeserializeByReflection<RawLogicDef>(node)).ToArray();

            _rawItemRandomizerLogic = locLogic.Select(l => l.ToItemLogic())
                .Concat(waypointLogic.Select(l => l.ToItemLogic()))
                .ToArray();
            _rawAreaRandomizerLogic = locLogic.Select(l => l.ToAreaLogic())
                .Concat(waypointLogic.Select(l => l.ToAreaLogic()))
                .Concat(areaTransitionLogic)
                .ToArray();
            _rawRoomRandomizerLogic = locLogic.Select(l => l.ToRoomLogic())
                .Concat(roomTransitionLogic)
                .ToArray();
            _waypointNames = new HashSet<string>(waypointLogic.Select(w => w.name));

            // Costs
            _costs = costs.Select(node => node.DeserializeByReflection<CostDef>()).ToArray();

            // Logic Settings
            _logicSettings = logic_settings.ToDictionary(node => node.GetNameAttribute(), node => node.DeserializeByReflection<LogicSettingDef>());

            // pools
            _pools = pools.ToDictionary(node => node.GetNameAttribute(), node => node["path"]?.InnerText);

            // vanillas
            _pooledVanillaDefs = vanilla.Select(node => node.DeserializeByReflection<VanillaDef>())
                .Where(def => def.pool != null)
                .GroupBy(def => def.pool)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }
    }
}
