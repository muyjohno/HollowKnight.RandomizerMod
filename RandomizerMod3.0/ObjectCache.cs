using System.Collections.Generic;
using Modding;
using UnityEngine;
using static RandomizerMod.LogHelper;
using SereCore;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace RandomizerMod
{
    internal static class ObjectCache
    {
        private static GameObject _shinyItem;

        private static GameObject _smallGeo;
        private static GameObject _mediumGeo;
        private static GameObject _largeGeo;

        private static GameObject _soul;

        private static GameObject _tinkEffect;

        private static GameObject _respawnMarker;

        private static GameObject _smallPlatform;

        private static GameObject _jinn;

        private static GameObject _relicGetMsg;

        private static GameObject _grubJar;

        private static GameObject _loreTablet;

        private static Dictionary<GeoRockSubtype, GameObject> _geoRocks;

        public static GameObject ShinyItem => Object.Instantiate(_shinyItem);

        public static GameObject SmallGeo => Object.Instantiate(_smallGeo);

        public static GameObject MediumGeo => Object.Instantiate(_mediumGeo);

        public static GameObject LargeGeo => Object.Instantiate(_largeGeo);

        public static GameObject Soul => Object.Instantiate(_soul);

        public static GameObject TinkEffect => Object.Instantiate(_tinkEffect);

        public static GameObject RespawnMarker => Object.Instantiate(_respawnMarker);

        public static GameObject SmallPlatform => Object.Instantiate(_smallPlatform);

        public static GameObject Jinn => Object.Instantiate(_jinn);

        public static GameObject RelicGetMsg => Object.Instantiate(_relicGetMsg);

        public static GameObject GrubJar => Object.Instantiate(_grubJar);

        public static GeoRockSubtype GetPreloadedRockType(GeoRockSubtype t) {
            return _geoRocks.ContainsKey(t) ? t : GeoRockSubtype.Default;
        }

        public static GameObject GeoRock(GeoRockSubtype t) {
            return Object.Instantiate(_geoRocks[t]);
        }

        public static GameObject Grub;
        public static AudioClip[] GrubCry;

        public static AudioClip LoreSound;

        public static void GetPrefabs(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            _shinyItem = objectsByScene[SceneNames.Tutorial_01]["_Props/Chest/Item/Shiny Item (1)"];
            _shinyItem.name = "Randomizer Shiny";

            PlayMakerFSM shinyFSM = _shinyItem.LocateMyFSM("Shiny Control");
            _relicGetMsg = Object.Instantiate(shinyFSM.GetState("Trink Flash").GetActionsOfType<SpawnObjectFromGlobalPool>()[1].gameObject.Value);
            _relicGetMsg.SetActive(false);
            Object.DontDestroyOnLoad(_relicGetMsg);

            HealthManager health = objectsByScene[SceneNames.Tutorial_01]["_Enemies/Crawler 1"].GetComponent<HealthManager>();
            _smallGeo = Object.Instantiate(
                ReflectionHelper.GetAttr<HealthManager, GameObject>(health, "smallGeoPrefab"));
            _mediumGeo =
                Object.Instantiate(ReflectionHelper.GetAttr<HealthManager, GameObject>(health, "mediumGeoPrefab"));
            _largeGeo = Object.Instantiate(
                ReflectionHelper.GetAttr<HealthManager, GameObject>(health, "largeGeoPrefab"));

            _smallGeo.SetActive(false);
            _mediumGeo.SetActive(false);
            _largeGeo.SetActive(false);
            Object.DontDestroyOnLoad(_smallGeo);
            Object.DontDestroyOnLoad(_mediumGeo);
            Object.DontDestroyOnLoad(_largeGeo);

            PlayMakerFSM fsm  = objectsByScene[SceneNames.Cliffs_02]["Soul Totem 5"].LocateMyFSM("soul_totem");
            _soul = Object.Instantiate(fsm.GetState("Hit").GetActionOfType<FlingObjectsFromGlobalPool>().gameObject.Value);
            _soul.SetActive(false);
            Object.DontDestroyOnLoad(_soul);

            _tinkEffect = Object.Instantiate(objectsByScene[SceneNames.Tutorial_01]["_Props/Cave Spikes (1)"].GetComponent<TinkEffect>().blockEffect);
            _tinkEffect.SetActive(false);
            Object.DontDestroyOnLoad(_tinkEffect);

            Object.Destroy(objectsByScene[SceneNames.Tutorial_01]["_Props/Cave Spikes (1)"]);
            Object.Destroy(objectsByScene[SceneNames.Tutorial_01]["_Enemies/Crawler 1"]);

            _respawnMarker = objectsByScene[SceneNames.Tutorial_01]["_Markers/Death Respawn Marker"];
            Object.DontDestroyOnLoad(_respawnMarker);

            _smallPlatform = objectsByScene[SceneNames.Tutorial_01]["_Scenery/plat_float_17"];
            Object.DontDestroyOnLoad(_smallPlatform);

            _grubJar = objectsByScene[SceneNames.Abyss_19]["Grub Bottle"];
            Object.DontDestroyOnLoad(_grubJar);

            if (RandomizerMod.Instance.globalSettings.ReducePreloads)
            {
                _geoRocks = new Dictionary<GeoRockSubtype, GameObject>() {
                    [GeoRockSubtype.Default] = objectsByScene[SceneNames.Tutorial_01]["_Props/Geo Rock 1"],
                };
            }
            else
            {
                _geoRocks = new Dictionary<GeoRockSubtype, GameObject>() {
                    [GeoRockSubtype.Default] = objectsByScene[SceneNames.Tutorial_01]["_Props/Geo Rock 1"],
                    [GeoRockSubtype.Abyss] = objectsByScene[SceneNames.Abyss_19]["Geo Rock Abyss"],
                    [GeoRockSubtype.City] = objectsByScene[SceneNames.Ruins2_05]["Geo Rock City 1"],
                    [GeoRockSubtype.Deepnest] = objectsByScene[SceneNames.Deepnest_02]["Geo Rock Deepnest"],
                    [GeoRockSubtype.Fung01] = objectsByScene[SceneNames.Fungus2_11]["Geo Rock Fung 01"],
                    [GeoRockSubtype.Fung02] = objectsByScene[SceneNames.Fungus2_11]["Geo Rock Fung 02"],
                    [GeoRockSubtype.Grave01] = objectsByScene[SceneNames.RestingGrounds_10]["Geo Rock Grave 01"],
                    [GeoRockSubtype.Grave02] = objectsByScene[SceneNames.RestingGrounds_10]["Geo Rock Grave 02"],
                    [GeoRockSubtype.GreenPath01] = objectsByScene[SceneNames.Fungus1_12]["Geo Rock Green Path 01"],
                    [GeoRockSubtype.GreenPath02] = objectsByScene[SceneNames.Fungus1_12]["Geo Rock Green Path 02"],
                    [GeoRockSubtype.Hive] = objectsByScene[SceneNames.Hive_01]["Geo Rock Hive"],
                    [GeoRockSubtype.Mine] = objectsByScene[SceneNames.Mines_20]["Geo Rock Mine (4)"],
                    [GeoRockSubtype.Outskirts] = objectsByScene[SceneNames.Deepnest_East_17]["Geo Rock Outskirts"],
                    [GeoRockSubtype.Outskirts420] = objectsByScene[SceneNames.Deepnest_East_17]["Giant Geo Egg"]
                };
            }
            
            foreach (var entry in _geoRocks) {
                Object.DontDestroyOnLoad(entry.Value);
            }

            Grub = objectsByScene[SceneNames.Abyss_19]["Grub Bottle/Grub"];
            GrubCry = Grub.LocateMyFSM("Grub Control").GetState("Leave").GetActionOfType<AudioPlayRandom>().audioClips;
            Object.DontDestroyOnLoad(Grub);
            foreach (AudioClip clip in GrubCry)
            {
                Object.DontDestroyOnLoad(clip);
            }

            _loreTablet = objectsByScene[SceneNames.Tutorial_01]["_Props/Tut_tablet_top"];
            LoreSound = (AudioClip)_loreTablet.LocateMyFSM("Inspection").GetState("Prompt Up").GetActionOfType<AudioPlayerOneShotSingle>().audioClip.Value;
            Object.DontDestroyOnLoad(LoreSound);

            _jinn = objectsByScene[SceneNames.Room_Jinn]["Jinn NPC"];
            Object.DontDestroyOnLoad(_jinn);

            if (_shinyItem == null || _smallGeo == null || _mediumGeo == null || _largeGeo == null ||
                _tinkEffect == null || _respawnMarker == null || _smallPlatform == null)
            {
                LogWarn("One or more ObjectCache items are null");
            }
        }
    }
}
