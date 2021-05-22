using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using RandomizerMod.Settings;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Randomization.Logic
{
    public class ProgressionManager : IManagedProgressionManager
    {
        public bool[] obtained;
        public Dictionary<CostType, int> intCounts = Enum.GetValues(typeof(CostType)).Cast<CostType>().ToDictionary(e => e, e => 0);

        private bool temp;
        public List<string> tempItems;

        public LogicManager LM { get; protected set; }
        private GenerationSettings GS;

        private int? Index(string item) => LM.GetIndex(item);
        private Cost? Value(string item) => LM.GetValue(item);
        

        /// <summary>
        /// Event arg is true if item was added temporarily
        /// </summary>
        public event Action<bool> AfterAddItem;

        /// <summary>
        /// Event arg is true if temp items were saved
        /// </summary>
        public event Action<bool> AfterEndTemp;

        public ProgressionManager(LogicManager lm, GenerationSettings gs)
        {
            this.LM = lm;
            this.GS = gs;
            obtained = new bool[lm.FlagCount];

            ApplySettings();
        }

        public bool Has(string item)
        {
            return Index(item) is int i && obtained[i];
        }

        public bool Has(int index)
        {
            return obtained[index];
        }


        public bool CompareCost(Cost cost)
        {
            int count = intCounts[cost.type];
            switch (cost.type)
            {
                case CostType.Grub:
                    return count >= cost.amt + GS.GrubCostRandomizerSettings.GrubTolerance;
                case CostType.Essence:
                    return count >= cost.amt + GS.EssenceCostRandomizerSettings.EssenceTolerance;
                default:
                    return count >= cost.amt;
            }
        }

        public bool CanGet(string item)
        {
            return LM.EvaluateLogic(item, this);
        }

        // TODO: restore parse logic w/ result
        /*
        public bool CanGetWithReqs(string item, out string reqs)
        {
            bool canGet = LM.ParseLogicWithResult(this, item, out int[] itemVals);

            if (canGet)
            {
                reqs = itemVals.Select(i => LM.InvertProgressionIndex(i)).Aggregate(string.Empty, (r, s) => r + " + " + s);
            }
            else reqs = null;

            return canGet;
        }
        */

        public void Add(string item)
        {
            if (temp)
            {
                tempItems.Add(item);
            }
            if (Index(item) is int i)
            {
                obtained[i] = true;
            }
            if (Value(item) is Cost cost)
            {
                intCounts[cost.type] += cost.amt;
            }

            UpdateWaypoints();
            AfterAddItem?.Invoke(temp);
        }

        // TODO: fix accessibility
        public void Add(StartDef start)
        {
            switch (LM.Mode)
            {
                case LogicMode.Room:
                    Add(start.roomTransition);
                    break;
                case LogicMode.Area:
                    Add(start.areaTransition);
                    goto case LogicMode.Item;
                case LogicMode.Item:
                    Add(start.waypoint);
                    break;
            }

            UpdateWaypoints();
        }

        public void Remove(string item)
        {
            if (Index(item) is int i)
            {
                obtained[i] = false;
            }
            if (Value(item) is Cost value)
            {
                intCounts[value.type] -= value.amt;
            }
        }

        public void Add(IEnumerable<string> items)
        {
            foreach (string item in items)
            {
                if (Index(item) is int i)
                {
                    obtained[i] = true;
                }
                if (Value(item) is Cost cost)
                {
                    intCounts[cost.type] += cost.amt;
                }
            }
            UpdateWaypoints();
            AfterAddItem?.Invoke(temp);
        }

        public void Remove(IEnumerable<string> items)
        {
            foreach (string item in items)
            {
                if (Index(item) is int i)
                {
                    obtained[i] = false;
                }
                if (Value(item) is Cost value)
                {
                    intCounts[value.type] -= value.amt;
                }
            }
        }

        public void AddTemp(string item)
        {
            temp = true;
            if (tempItems == null)
            {
                tempItems = new List<string>();
            }
            Add(item);
        }

        public void RemoveTempItems()
        {
            temp = false;
            Remove(tempItems);
            tempItems = new List<string>();
            AfterEndTemp?.Invoke(false);
        }

        public void SaveTempItems()
        {
            temp = false;

            tempItems = new List<string>();
            AfterEndTemp?.Invoke(true);
        }

        private void ApplySettings()
        {
            foreach (string setting in SkipSettings.FieldNames)
            {
                if (GS.SkipSettings.GetFieldByName(setting)) Add(setting.ToUpper());
            }

            // TODO: non-skip settings in PM
            /*
            if (!settings.Cursed) Add("NOTCURSED");
            if (settings.Cursed) Add("CURSED");
            */
        }

        public void UpdateWaypoints()
        {
            foreach(string waypoint in LM.WaypointNames)
            {
                if (!Has(waypoint) && CanGet(waypoint))
                {
                    Add(waypoint);
                }
            }
        }
    }
}
