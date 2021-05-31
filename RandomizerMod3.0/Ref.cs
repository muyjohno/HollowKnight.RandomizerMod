using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;


namespace RandomizerMod
{
    public static class Ref
    {
        public static RandomizerSettings SET => RandomizerMod.Instance._settings;
        public static GenerationSettings GEN => SET.GenerationSettings;
        public static SkipSettings SKIP => GEN.SkipSettings;
        public static PoolSettings POOL => GEN.PoolSettings;
        public static CursedSettings CURSE => GEN.CursedSettings;
        public static SaveData SD => SET.SaveData;
        public static CustomSkillSaveData SKILLS => SD.CustomSkills;
        public static PlacementSaveData PLACEMENTS => SD.Placements;
        public static EventSaveData EVENTS => SD.Events;
        public static QoLSettings GME => SET.GameSettings;

        public static PlayerData PD => PlayerData.instance;
        public static GameManager GM => GameManager.instance;
        public static HeroController HC => HeroController.instance;


    }
}
