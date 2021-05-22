using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class CostDef
    {
        public CostType type;
        public int amt;
        public bool randomizable;
        public string location;

        public Randomization.Logic.Cost GetCost()
        {
            return new Randomization.Logic.Cost(type, amt);
        }
    }
}
