using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Randomization
{
    public class ProgressionManager
    {
        public int[] obtained;
        public ProgressionManager(int[] progression = null, bool addSettings = true)
        {
            obtained = new int[LogicManager.bitMaskMax + 1];
            if (progression != null) progression.CopyTo(obtained, 0);
            if (addSettings) ApplyDifficultySettings();
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
        }

        public void Remove(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] &= ~a.Item1;
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
            if (RandomizerMod.Instance.Settings.MiscSkips) Add("MISCSKIPS");
            if (RandomizerMod.Instance.Settings.FireballSkips) Add("FIREBALLSKIPS");
            if (RandomizerMod.Instance.Settings.DarkRooms) Add("DARKROOMS");
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
