using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    public class PlacementSaveData
    {
        // Item Randomizer
        public (string item, string location)[] ItemPlacements;

        // Cost Randomizer
        public Dictionary<int, (CostType type, int amt)> Costs;

        // Transition Randomizer
        public Dictionary<string, string> TransitionPlacements;

        public int GetId(string item, string location)
        {
            return Array.IndexOf(ItemPlacements, (item, location));
        }

        public bool TryGetTransition(string fromScene, string fromGate, out string toScene, out string toGate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// List, in order, of obtained items. Items are the effective given items, not necessarily the placed items.
        /// </summary>
        public List<(string item, string location)> EffectiveObtained = new List<(string item, string location)>();

        /// <summary>
        /// Obtain flag for each item-location pair in PlacementSaveData. Read using IsObtained.
        /// </summary>
        public bool[] ObtainedPlacements;
        public HashSet<string> ObtainedLocations;

        public bool IsObtained(int id)
        {
            return ObtainedPlacements[id];
        }

        public bool CheckItemFound(string item)
        {
            throw new NotImplementedException();
        }

        public bool CheckLocationFound(string location)
        {
            return ObtainedLocations.Contains(location);
        }
    }
}
