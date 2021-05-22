using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    public class CompletionSaveData
    {
        /// <summary>
        /// List, in order, of obtained items. Items are the effective given items, not necessarily the placed items.
        /// </summary>
        public List<string> EffectiveObtainedItems = new List<string>();

        /// <summary>
        /// Obtain flag for each item-location pair in PlacementSaveData. Read using IsObtained.
        /// </summary>
        public bool[] ObtainedPlacements;

        public bool IsObtained(int id)
        {
            return ObtainedPlacements[id];
        }


        // Miscellaneous
        public int TotalFlamesCollected;
    }
}
