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
    public class RandomizerContext
    {
        public LogicManager LM;
        public GenerationSettings GS;

        // Result
        public StartDef Start;
        public List<ILP> ItemPlacements = new List<ILP>();
        public Dictionary<string, string> TransitionPlacements = new Dictionary<string, string>();
        public int[] NotchCosts;
        public int StartGeo;

        // Progress
        public List<string> UnplacedItems;
        public List<string> UnfilledLocations;
        public List<string> UnmatchedTransitions;

        /// <summary>
        /// The list of fixed item placements; i.e. vanilla placements and others that should not be cleared on randomizer failure, etc.
        /// </summary>
        public List<ILP> FixedItemPlacements;
        /// <summary>
        /// The list of fixed transition placements; i.e. vanilla placements and others that should not be cleared on randomizer failure, etc.
        /// </summary>
        public Dictionary<string, string> FixedTransitionPlacements;

        public Dictionary<string, Cost> overrideCosts = new Dictionary<string, Cost>();

        public RandomizerContext(GenerationSettings gs)
        {
            GS = gs;
            LogicMode mode;
            switch (GS.TransitionSettings.Mode)
            {
                default:
                case TransitionSettings.TransitionMode.None:
                    mode = LogicMode.Item;
                    break;
                case TransitionSettings.TransitionMode.AreaRandomizer:
                    mode = LogicMode.Area;
                    break;
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    mode = LogicMode.Room;
                    break;
            }
            LM = new LogicManager(mode, Data.GetRawLogicDefsByMode(mode));

            UnplacedItems = GS.GetRandomizedItems();
            UnfilledLocations = GS.GetRandomizedLocations();
            UnmatchedTransitions = GS.GetRandomizedTransitions();

            FixedItemPlacements = GS.GetVanillaProgression(LM);
            FixedTransitionPlacements = new Dictionary<string, string>();
    }



        public void OverrideCost(string location, Cost cost)
        {
            LM.AddNewCostOverride(location, cost);
            overrideCosts[location] = cost;
        }


        public (ProgressionManager, PrePlacedManager) ApplyContext()
        {
            ProgressionManager pm = new ProgressionManager(LM, GS);
            if (Start != null) pm.Add(Start);

            List<ILP> trackingPairs = ItemPlacements
                .Concat(FixedItemPlacements)
                .Concat(FixedTransitionPlacements
                    .SelectMany(kvp => new ILP[] { (kvp.Key, kvp.Key), (kvp.Key, kvp.Value) }))
                .Concat(TransitionPlacements
                    .SelectMany(kvp => new ILP[] { (kvp.Key, kvp.Key), (kvp.Key, kvp.Value) }))
                .ToList();

            PrePlacedManager ppm = new PrePlacedManager(trackingPairs, pm);

            return (pm, ppm);
        }
    }
}
