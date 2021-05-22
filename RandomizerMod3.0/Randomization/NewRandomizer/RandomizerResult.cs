using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using RandomizerMod.Randomization.Util;
using RandomizerMod.Randomization.Logic;
using LogicManager = RandomizerMod.Randomization.Logic.LogicManager;

namespace RandomizerMod.Randomization.NewRandomizer
{
    public class RandomizerResult
    {
        public LogicManager LM;
        public GenerationSettings GS;


        public StartDef Start;
        public List<ILP> ItemPlacements = new List<ILP>();
        
        public Dictionary<string, string> TransitionPlacements = new Dictionary<string, string>();
        public int[] NotchCosts;

        public Dictionary<string, Cost> overrideCosts = new Dictionary<string, Cost>();


        public void OverrideCost(string location, Cost cost)
        {
            LM.AddNewCostOverride(location, cost);
            overrideCosts[location] = cost;
        }


        public (ProgressionManager, PrePlacedManager) ApplyContext()
        {
            ProgressionManager pm = new ProgressionManager(LM, GS);

            List<ILP> trackingPairs = ItemPlacements
                .Concat(TransitionPlacements
                    .SelectMany(kvp => new ILP[] { (kvp.Key, kvp.Key), (kvp.Key, kvp.Value) })).ToList();

            PrePlacedManager ppm = new PrePlacedManager(trackingPairs, pm);

            return (pm, ppm);
        }
    }
}
