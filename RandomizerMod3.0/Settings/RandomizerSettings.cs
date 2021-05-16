using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;

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
        public (string item, string location)[] placements;

        public CustomSkillSaveData CustomSkills = new CustomSkillSaveData();
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
