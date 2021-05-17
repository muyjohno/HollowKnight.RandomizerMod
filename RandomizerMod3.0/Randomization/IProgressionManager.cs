using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    public interface IProgressionManager
    {
        // TODO: Make PM implement this interface
        // TODO: Replace the old bitmask with a more sensible system

        [Obsolete]
        bool Has(int flag, int sheet);

        bool Has(int id);
        bool Has(string item);
        bool CanGet(string item);
        bool CompareEssence(int value);
        bool CompareGrubs(int value);
        bool CompareFlames(int value);




    }
}
