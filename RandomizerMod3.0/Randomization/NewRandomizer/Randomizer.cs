using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;
using RandomizerMod.Randomization.Logic;

namespace RandomizerMod.Randomization.NewRandomizer
{
    public interface IItemRandomizer
    {
        void PlaceItems(RandomizerContext ctx);
    }

    public interface ITransitionRandomizer
    {
        void PlaceTransitions(RandomizerContext ctx);
    }

    public class Randomizer
    {
        Randomizer3 ItemRandomizer;
        CostRandomizer CostRandomizer;
        StartRandomizer StartRandomizer;
        LogicManager LM;

        public Randomizer()
        {
            ItemRandomizer = new Randomizer3();
            CostRandomizer = new CostRandomizer();
            StartRandomizer = new StartRandomizer();
        }

        public RandomizerContext Randomize(GenerationSettings GS)
        {
            RandomizerContext ctx = new RandomizerContext(GS);
            Random rng = new Random(GS.Seed);

            GS.CursedSettings.HandleRandomCurses(rng);

            CostRandomizer.SetContext(GS, ctx, rng);
            CostRandomizer.HandleGrubCosts();
            CostRandomizer.HandleEssenceCosts();

            if (GS.MiscSettings.RandomizeNotchCosts)
            {
                ctx.NotchCosts = NotchCostRandomizer.Randomize(rng);
            }

            StartRandomizer.SetContext(GS, ctx, rng);
            StartRandomizer.RandomizeStartingLocation();
            StartRandomizer.RandomizeStartingItems();

            ItemRandomizer.SetContext(GS, ctx, rng);
            ItemRandomizer.RandomizeItems(ctx);

            return ctx;
        }

    }
}
