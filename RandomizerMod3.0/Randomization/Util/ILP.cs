using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization.Util
{
    public struct ILP
    {
        public string item;
        public string location;

        public ILP(string item, string location)
        {
            this.item = item;
            this.location = location;
        }

        public static implicit operator (string item, string location)(ILP ilp) => (ilp.item, ilp.location);
        public static implicit operator ILP((string item, string location) ilp) => new ILP(ilp.item, ilp.location);
    }
}
