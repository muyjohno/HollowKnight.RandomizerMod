using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Components;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using static RandomizerMod.LogHelper;
using System.Collections;
using RandomizerMod.Randomization;
using System;
using Object = UnityEngine.Object;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod
{
    public static class OpenMode
    {
        public const string RESPAWN_MARKER_NAME = "Death Respawn Marker";
        public const string RESPAWN_TAG = "RespawnPoint";

        public static readonly Dictionary<string, Vector2> StartLocations = new Dictionary<string, Vector2>
        {
            { SceneNames.Ruins1_27, new Vector2(29.6f, 6.4f) },
            { SceneNames.Cliffs_03, new Vector2(85.8f, 46.4f) },
            { SceneNames.Room_spider_small, new Vector2(23.1f, 13.4f) },
            { SceneNames.Hive_03, new Vector2(47.2f, 142.4f) },
            { SceneNames.Mines_34, new Vector2(128.3f, 46.4f) },
            { SceneNames.Fungus2_30, new Vector2(64.8f, 21.4f) },
            { SceneNames.Fungus3_25, new Vector2(36.2f, 32.4f) },
            { SceneNames.Deepnest_East_15, new Vector2(26.5f, 4.4f) },
            { SceneNames.Waterways_03, new Vector2(93.6f, 4.4f) },
            { SceneNames.Abyss_06_Core, new Vector2(42f, 5.4f) },
            { SceneNames.Fungus3_49, new Vector2(25.3f, 6.4f) }
        };

        private static readonly Dictionary<string, MapZone> StartZones = new Dictionary<string, MapZone>
        {
            { SceneNames.Ruins1_27, MapZone.CITY },
            { SceneNames.Cliffs_03, MapZone.CLIFFS },
            { SceneNames.Room_spider_small, MapZone.DEEPNEST },
            { SceneNames.Hive_03, MapZone.HIVE },
            { SceneNames.Mines_34, MapZone.MINES },
            { SceneNames.Fungus2_30, MapZone.WASTES },
            { SceneNames.Fungus3_25, MapZone.FOG_CANYON },
            { SceneNames.Deepnest_East_15, MapZone.OUTSKIRTS },
            { SceneNames.Waterways_03, MapZone.WATERWAYS },
            { SceneNames.Abyss_06_Core, MapZone.ABYSS },
            { SceneNames.Fungus3_49, MapZone.ROYAL_GARDENS }
        };

        private static void CreateRespawnMarker(string sceneName)
        {
            GameObject respawnMarker = ObjectCache.RespawnMarker;
            respawnMarker.transform.SetPosition2D(StartLocations[sceneName]);
            respawnMarker.transform.SetPositionZ(7.4f);
            respawnMarker.name = RESPAWN_MARKER_NAME;
            respawnMarker.tag = RESPAWN_TAG;
            respawnMarker.SetActive(true);
        }

        public static void OpenModeSceneChanges(Scene newScene)
        {
            if (!RandomizerMod.Instance.Settings.OpenMode) return;
            if (newScene.name != RandomizerMod.Instance.Settings.StartLocation) return;

            CreateRespawnMarker(newScene.name);

            switch (newScene.name)
            {
                case SceneNames.Ruins1_27:
                    PlayerData.instance.hornetFountainEncounter = true;
                    Object.Destroy(GameObject.Find("Fountain Inspect"));
                    break;
                
                case SceneNames.Fungus3_25:
                    Object.Destroy(GameObject.Find("Cornifer"));
                    Object.Destroy(GameObject.Find("Cornifer Card"));
                    break;
                case SceneNames.Abyss_06_Core:
                    PlayerData.instance.abyssGateOpened = true;
                    break;
            }
        }

        public static void OpenModeDataChanges()
        {
            PlayerData.instance.Reset();

            PlayerData.instance.hasCharm = true;

            Ref.PD.unchainedHollowKnight = true;
            Ref.PD.encounteredMimicSpider = true;
            Ref.PD.infectedKnightEncountered = true;
            Ref.PD.mageLordEncountered = true;
            Ref.PD.mageLordEncountered_2 = true;

            PlayerData.instance.openedTownBuilding = true;
            PlayerData.instance.openedCrossroads = true;
            PlayerData.instance.openedGreenpath = true;
            PlayerData.instance.openedFungalWastes = true;
            PlayerData.instance.openedGardensStagStation = true;
            PlayerData.instance.openedRuins1 = true;
            PlayerData.instance.openedRuins2 = true;
            PlayerData.instance.openedRestingGrounds = true;
            PlayerData.instance.openedDeepnest = true;
            PlayerData.instance.openedHiddenStation = true;
            PlayerData.instance.openedStagNest = true;

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

            PlayerData.instance.respawnScene = RandomizerMod.Instance.Settings.StartLocation;
            PlayerData.instance.respawnMarkerName = RESPAWN_MARKER_NAME;
            PlayerData.instance.respawnType = 0;
            PlayerData.instance.mapZone = StartZones.TryGetValue(RandomizerMod.Instance.Settings.StartLocation, out MapZone zone) ? zone : MapZone.KINGS_PASS;
        }
    }
}
