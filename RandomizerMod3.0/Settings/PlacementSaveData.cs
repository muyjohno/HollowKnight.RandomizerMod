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
        public Dictionary<int, (int cost, int costType)> Costs;

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

    }
}
