using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization.Logic
{
    public struct Cost
    {
        public int amt;
        public CostType type;

        public Cost(CostType type, int amt)
        {
            this.amt = amt;
            this.type = type;
        }

        public static implicit operator (CostType type, int amt)(Cost cost) => (cost.type, cost.amt);
        public static implicit operator Cost((CostType type, int amt) p) => new Cost(p.type, p.amt);
    }
}
