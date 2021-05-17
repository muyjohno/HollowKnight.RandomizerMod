using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    public class CompletionSaveData
    {
        public Dictionary<string, int> AdditiveCounts;
        // TODO: Structure for storing order in which items are first obtained


        public bool[] ObtainedItems;

        public bool IsObtained(int id)
        {
            return ObtainedItems[id];
        }

    }
}
