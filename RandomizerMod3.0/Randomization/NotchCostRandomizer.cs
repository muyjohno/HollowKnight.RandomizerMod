using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    public static class NotchCostRandomizer
    {
        public static int[] Randomize(Random rng)
        {
            int[] costs = new int[40];
            for (int total = rng.Next(80, 160); total > 0; total--)
            {
                while (rng.Next(40) is int i && costs[i] <= 6)
                {
                    costs[i]++;
                }
            }
            return costs;
        }

        public static void Apply(int[] newCosts)
        {
            for (int i = 0; i < 40; i++)
            {
                PlayerData.instance.SetInt($"charmCost_{i + 1}", newCosts[i]);
            }
        }
    }
}
