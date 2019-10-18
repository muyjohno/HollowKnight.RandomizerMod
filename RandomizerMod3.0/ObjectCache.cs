using System.Collections.Generic;
using Modding;
using UnityEngine;
using static RandomizerMod.LogHelper;

namespace RandomizerMod
{
    internal static class ObjectCache
    {
        private static GameObject _shinyItem;

        private static GameObject _smallGeo;
        private static GameObject _mediumGeo;
        private static GameObject _largeGeo;

        private static GameObject _tinkEffect;

        private static GameObject _respawnMarker;

        private static GameObject _smallPlatform;

        public static GameObject ShinyItem => Object.Instantiate(_shinyItem);

        public static GameObject SmallGeo => Object.Instantiate(_smallGeo);

        public static GameObject MediumGeo => Object.Instantiate(_mediumGeo);

        public static GameObject LargeGeo => Object.Instantiate(_largeGeo);

        public static GameObject TinkEffect => Object.Instantiate(_tinkEffect);

        public static GameObject RespawnMarker => Object.Instantiate(_respawnMarker);

        public static GameObject SmallPlatform => Object.Instantiate(_smallPlatform);

        public static void GetPrefabs(Dictionary<string, GameObject> objects)
        {
            _shinyItem = objects["_Props/Chest/Item/Shiny Item (1)"];
            _shinyItem.name = "Randomizer Shiny";

            HealthManager health = objects["_Enemies/Crawler 1"].GetComponent<HealthManager>();
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

            _tinkEffect = Object.Instantiate(objects["_Props/Cave Spikes (1)"].GetComponent<TinkEffect>().blockEffect);
            _tinkEffect.SetActive(false);
            Object.DontDestroyOnLoad(_tinkEffect);

            Object.Destroy(objects["_Props/Cave Spikes (1)"]);
            Object.Destroy(objects["_Enemies/Crawler 1"]);

            _respawnMarker = objects["_Markers/Death Respawn Marker"];
            Object.DontDestroyOnLoad(_respawnMarker);

            _smallPlatform = objects["_Scenery/plat_float_17"];
            Object.DontDestroyOnLoad(_smallPlatform);

            if (_shinyItem == null || _smallGeo == null || _mediumGeo == null || _largeGeo == null ||
                _tinkEffect == null || _respawnMarker == null || _smallPlatform == null)
            {
                LogWarn("One or more ObjectCache items are null");
            }
        }
    }
}
