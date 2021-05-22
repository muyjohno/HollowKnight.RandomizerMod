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
        void PlaceItems(RandomizerResult ctx);
    }

    public interface ITransitionRandomizer
    {
        void PlaceTransitions(RandomizerResult ctx);
    }

    public class Randomizer
    {
        Randomizer3 ItemRandomizer;
        CostRandomizer CostRandomizer;
        LogicManager LM;

        public Randomizer()
        {
            ItemRandomizer = new Randomizer3();
            CostRandomizer = new CostRandomizer();
        }

        public RandomizerResult Randomize(GenerationSettings GS)
        {
            // TODO: Get LM from pool
            RandomizerResult ctx = new RandomizerResult();
            Random rng = new Random(GS.Seed);

            CostRandomizer.SetContext(GS, ctx, rng);
            CostRandomizer.RandomizeGrubCosts();
            CostRandomizer.RandomizeEssenceCosts();


            if (GS.CursedSettings.RandomCurses)
            {
                foreach (string name in Settings.Util.GetFieldNames(typeof(CursedSettings)))
                {
                    if (name == nameof(CursedSettings.RandomCurses)) continue;

                    if (Settings.Util.Get(GS.CursedSettings, name) is bool value && value)
                    {
                        if (rng.Next(0, 2) == 0) Settings.Util.Set(GS.CursedSettings, name, false);
                    }
                }
            }

            if (GS.MiscSettings.RandomizeNotchCosts)
            {
                ctx.NotchCosts = NotchCostRandomizer.Randomize(rng);
            }

            if (GS.StartLocationSettings.StartLocationType != StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                // TODO: randomize start location
            }

            // TODO: randomize start items

            



            return ctx;
        }

    }
}
