using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;
using RandomizerMod.RandomizerData;
using RandomizerMod.Randomization.Logic;

namespace RandomizerMod.Randomization.NewRandomizer
{
    public class CostRandomizer
    {
        public CostRandomizer() { }

        public GenerationSettings GS;
        public RandomizerContext CTX;
        public Random RNG;

        public void SetContext(GenerationSettings gs, RandomizerContext ctx, Random rng)
        {
            GS = gs;
            CTX = ctx;
            RNG = rng;
        }

        public void HandleGrubCosts()
        {
            if (GS.GrubCostRandomizerSettings.RandomizeGrubItemCosts)
            {
                foreach (CostDef def in Data.GetCostDefs().Where(def => def.type == CostType.Grub && def.randomizable))
                {
                    int newCost = RNG.Next(GS.GrubCostRandomizerSettings.MinimumGrubCost, GS.GrubCostRandomizerSettings.MaximumGrubCost + 1);
                    CTX.OverrideCost(def.location, new Cost(CostType.Grub, newCost));
                }
            }
        }

        public void HandleEssenceCosts()
        {
            if (GS.EssenceCostRandomizerSettings.RandomizeEssenceItemCosts)
            {
                foreach (CostDef def in Data.GetCostDefs().Where(def => def.type == CostType.Essence && def.randomizable))
                {
                    int newCost = RNG.Next(GS.EssenceCostRandomizerSettings.MinimumEssenceCost, GS.EssenceCostRandomizerSettings.MaximumEssenceCost + 1);
                    CTX.OverrideCost(def.location, new Cost(CostType.Essence, newCost));
                }
            }
        }



    }
}
