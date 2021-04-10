using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Components;
using RandomizerMod.FsmStateActions;
using SereCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using static RandomizerMod.LogHelper;
using System.Collections;
using RandomizerMod.Randomization;
using System;
using Object = UnityEngine.Object;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod
{
    public static class StartSaveChanges
    {
        public const string RESPAWN_MARKER_NAME = "Randomizer Respawn Marker";
        public const string RESPAWN_TAG = "RespawnPoint";
        private static StartDef start => LogicManager.GetStartLocation(RandomizerMod.Instance.Settings.StartName);

        private static void CreateRespawnMarker()
        {
            GameObject respawnMarker = ObjectCache.RespawnMarker;
            respawnMarker.transform.SetPosition2D(start.x, start.y);
            respawnMarker.transform.SetPositionZ(7.4f);
            respawnMarker.name = RESPAWN_MARKER_NAME;
            respawnMarker.tag = RESPAWN_TAG;
            respawnMarker.SetActive(true);
        }

        // Merge or move this switch block into scene changes
        public static void StartSceneChanges(Scene newScene)
        {
            if (newScene.name != start.sceneName) return;

            CreateRespawnMarker();

            switch (newScene.name)
            {
                case SceneNames.Ruins1_27:
                    PlayerData.instance.hornetFountainEncounter = true;
                    Object.Destroy(GameObject.Find("Fountain Inspect"));
                    break;

                case SceneNames.Abyss_06_Core:
                    PlayerData.instance.abyssGateOpened = true;
                    break;
            }
        }

        public static void StartDataChanges()
        {
            PlayerData.instance.Reset();

            PlayerData.instance.hasCharm = true;

            /*
            if (RandomizerMod.Instance.Settings.FreeLantern)
            {
                PlayerData.instance.hasLantern = true;
            }
            */

            if (RandomizerMod.Instance.Settings.EarlyGeo)
            {
                // added version checking to the early geo randomization
                int geoSeed = RandomizerMod.Instance.Settings.Seed;
                unchecked
                {
                    geoSeed = geoSeed * 17 + 31 * RandomizerMod.Instance.MakeAssemblyHash();
                }

                // added settings checking to early geo randomization
                // split away difficulty settings etc from randomization settings so we have a bit more room
                int randomizationSettingsSeed = 0;
                if (RandomizerMod.Instance.Settings.RandomizeDreamers) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeSkills) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeCharms) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeKeys) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeGeoChests) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeMaskShards) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeVesselFragments) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeCharmNotches) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizePaleOre) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeRancidEggs) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeRelics) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeMaps) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeStags) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeGrubs) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeWhisperingRoots) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeRocks) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeSoulTotems) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeLoreTablets) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizePalaceTotems 
                    || RandomizerMod.Instance.Settings.RandomizePalaceTablets) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeLifebloodCocoons) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeGrimmkinFlames) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeBossEssence) randomizationSettingsSeed += 1;
                randomizationSettingsSeed = randomizationSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeBossGeo) randomizationSettingsSeed += 1;

                int miscSettingsSeed = 0;
                if (RandomizerMod.Instance.Settings.DuplicateMajorItems) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.ShadeSkips) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.AcidSkips) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.SpikeTunnels) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.MildSkips) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.SpicySkips) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.FireballSkips) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.DarkRooms) miscSettingsSeed += 1;
                miscSettingsSeed = miscSettingsSeed << 1;
                if (RandomizerMod.Instance.Settings.RandomizeClawPieces) miscSettingsSeed += 1;
                miscSettingsSeed <<= 1; 
                if (RandomizerMod.Instance.Settings.RandomizeCloakPieces) miscSettingsSeed += 1;
                miscSettingsSeed <<= 1;
                if (RandomizerMod.Instance.Settings.CursedNail) miscSettingsSeed += 1;
                miscSettingsSeed <<= 1;
                if (RandomizerMod.Instance.Settings.RandomizeRooms) miscSettingsSeed += 1;
                miscSettingsSeed <<= 1;
                if (RandomizerMod.Instance.Settings.RandomizeAreas || RandomizerMod.Instance.Settings.ConnectAreas) miscSettingsSeed += 1;

                int settingsSeed = 0;
                unchecked
                {
                    settingsSeed = 59 * randomizationSettingsSeed + 97 * miscSettingsSeed;
                }

                Random rand = new Random(geoSeed + settingsSeed);
                int startgeo = rand.Next(300, 600);
                PlayerData.instance.AddGeo(startgeo);
            }

            Ref.PD.unchainedHollowKnight = true;
            Ref.PD.encounteredMimicSpider = true;
            Ref.PD.infectedKnightEncountered = true;
            Ref.PD.mageLordEncountered = true;
            Ref.PD.mageLordEncountered_2 = true;
            Ref.PD.godseekerUnlocked = true;

            List<string> startItems = RandomizerMod.Instance.Settings.ItemPlacements.Where(pair => pair.Item2.StartsWith("Equip")).Select(pair => pair.Item1).ToList();
            foreach (string item in startItems)
            {
                GiveAction action = LogicManager.GetItemDef(item).action;
                if (action == GiveAction.Charm) action = GiveAction.EquippedCharm;
                else if (action == GiveAction.SpawnGeo) action = GiveAction.AddGeo;

                GiveItem(action, item, "Equipped");
            }

            for (int i = 1; i < 5; i++)
            {
                if (PlayerData.instance.charmSlotsFilled > PlayerData.instance.charmSlots)
                {
                    PlayerData.instance.charmSlots++;
                    PlayerData.instance.SetBool("salubraNotch" + i, true);
                }
                if (PlayerData.instance.charmSlotsFilled <= PlayerData.instance.charmSlots)
                {
                    PlayerData.instance.overcharmed = false;
                    break;
                }
            }

            PlayerData.instance.respawnScene = start.sceneName;
            PlayerData.instance.respawnMarkerName = RESPAWN_MARKER_NAME;
            PlayerData.instance.respawnType = 0;
            PlayerData.instance.mapZone = start.zone;
        }
    }
}
