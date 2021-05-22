using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization.Logic
{
    public interface IProgressionManager
    {
        void Add(string item);
        void Add(IEnumerable<string> items);
        void Remove(string item);
        void Remove(IEnumerable<string> items);
        bool Has(string item);
        bool CanGet(string item);
        bool CompareCost(Cost cost);
    }

    public interface IManagedProgressionManager : IProgressionManager
    {
        LogicManager LM { get; }

        bool Has(int id);
    }

    public static class IProgressionManagerExtensions
    {
        // Default implementations of methods go here

        public static bool CompareCost(this IProgressionManager pm, int amt, CostType type)
        {
            return pm.CompareCost(new Cost { amt = amt, type = type });
        }

        public static bool CompareEssence(this IProgressionManager pm, int amt)
        {
            return pm.CompareCost(amt, CostType.Essence);
        }

        public static bool CompareGrubs(this IProgressionManager pm, int amt)
        {
            return pm.CompareCost(amt, CostType.Grub);
        }

        public static bool CompareSimple(this IProgressionManager pm, int amt)
        {
            return pm.CompareCost(amt, CostType.Simple);
        }

        public static bool CompareFlames(this IProgressionManager pm, int amt)
        {
            return pm.CompareCost(amt, CostType.Flame);
        }
    }

}
