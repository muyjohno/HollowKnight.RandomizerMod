using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization.Logic
{
    public interface ILogicDef
    {
        bool Evaluate(IProgressionManager pm);

        ILogicDef And(string token);
        ILogicDef Or(string token);
        ILogicDef And(Cost cost);
    }

    public interface IManagedLogicDef : ILogicDef
    {
        bool FastEvaluate(IManagedProgressionManager pm);
    }

}
