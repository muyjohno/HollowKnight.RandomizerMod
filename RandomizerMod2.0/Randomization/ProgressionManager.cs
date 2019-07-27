using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    public class ProgressionManager
    {
        public int[] obtained;
        private List<string> grubItems;
        private List<string> essenceItems;

        public ProgressionManager(int[] progression = null, bool addSettings = true)
        {
            obtained = new int[LogicManager.bitMaskMax + 1];
            if (progression != null) progression.CopyTo(obtained, 0);
            if (addSettings) ApplyDifficultySettings();
            if (RandomizerMod.Instance.Settings.RandomizeRooms)
            {
                Add("Dream_Nail");
                Add("Dream_Gate");
            }
            RecalculateEssence();
            RecalculateGrubs();
        }

        public bool CanGet(string item)
        {
            return LogicManager.ParseProcessedLogic(item, obtained);
        }

        public void Add(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] |= a.Item1;
            RecalculateEssence();
            RecalculateGrubs();
        }

        public void Remove(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] &= ~a.Item1;
            RecalculateEssence();
            RecalculateGrubs();
        }

        public bool Has(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return false;
            }
            return (obtained[a.Item2] & a.Item1) == a.Item1;
        }

        public void ApplyDifficultySettings()
        {
            if (RandomizerMod.Instance.Settings.ShadeSkips) Add("SHADESKIPS");
            if (RandomizerMod.Instance.Settings.AcidSkips) Add("ACIDSKIPS");
            if (RandomizerMod.Instance.Settings.SpikeTunnels) Add("SPIKETUNNELS");
            if (RandomizerMod.Instance.Settings.SpicySkips) Add("SPICYSKIPS");
            if (RandomizerMod.Instance.Settings.FireballSkips) Add("FIREBALLSKIPS");
            if (RandomizerMod.Instance.Settings.DarkRooms) Add("DARKROOMS");
        }

        public void RecalculateEssence()
        {
            int essence = 0;
            if (essenceItems == null) essenceItems = LogicManager.ItemNames.Where(item => LogicManager.GetItemDef(item).pool.StartsWith("Essence")).ToList();

            foreach (string item in essenceItems)
            {
                if (CanGet(item))
                {
                    essence += LogicManager.GetItemDef(item).geo;
                }
                if (essence >= 900) break;
            }
            obtained[LogicManager.essenceIndex] = essence;
        }

        public void RecalculateGrubs()
        {
            int grubs = 0;
            if (grubItems == null) grubItems = LogicManager.ItemNames.Where(item => LogicManager.GetItemDef(item).pool == "Grub").ToList();

            foreach (string item in grubItems)
            {
                if (CanGet(item))
                {
                    grubs++;
                }
                if (grubs >= 23) break;
            }
            obtained[LogicManager.grubIndex] = grubs;
        }

        // useful for debugging
        public string ListObtainedProgression()
        {
            string progression = string.Empty;
            foreach (string item in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(item).progression && Has(item)) progression += item + ", ";
            }
            foreach (string transition in LogicManager.TransitionNames())
            {
                if (Has(transition)) progression += transition + ", ";
            }
            return progression;
        }
    }
}
