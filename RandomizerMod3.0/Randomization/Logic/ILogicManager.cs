using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization.Logic
{
    interface ILogicManager
    {
        bool TryGetLogicDef(string name, out ILogicDef def);
    }
}
