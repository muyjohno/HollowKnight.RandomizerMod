using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using static RandomizerMod.LogHelper;
using System.Reflection;

namespace RandomizerMod.Settings
{
    public class RandomizerSettings : ModSettings
    {
        public GameSettings GameSettings = new GameSettings();
        public GenerationSettings GenerationSettings = new GenerationSettings();
        public SaveData SaveData = new SaveData();
    }

    public class SaveData
    {
        public PlacementSaveData Placements = new PlacementSaveData();
        public CustomSkillSaveData CustomSkills = new CustomSkillSaveData();
        public CompletionSaveData Completion = new CompletionSaveData();
    }

    public class GameSettings
    {
        public bool RealGeoRocks;
        public bool PreloadGeoRocks;

        public bool JinnAppearsWithJiji;
        public bool PreloadJinn;

        public bool NPCItemDialogue;
        public bool RealGrubJars;
    }
}
