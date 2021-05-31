using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Modding;
using RandomizerMod.Actions;
using RandomizerMod.Randomization;
using SereCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using RandomizerMod.Settings;
using RandomizerMod.SceneChanges;
using System.Security.Cryptography;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod
    {
        private static Thread _logicParseThread;

        public static RandomizerMod Instance { get; private set; }

        public GlobalSettings globalSettings { get; set; } = new GlobalSettings();
        [Obsolete]
        public SaveSettings Settings { get; set; } = new SaveSettings();

        public RandomizerSettings _settings;

        public override ModSettings SaveSettings
        {
            get => Settings = Settings ?? new SaveSettings();
            set => Settings = value is SaveSettings saveSettings ? saveSettings : Settings;
        }

        public override ModSettings GlobalSettings
        {
            get => globalSettings = globalSettings ?? new GlobalSettings();
            set => globalSettings = value is GlobalSettings gSettings ? gSettings : globalSettings;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloaded)
        {
            if (Instance != null)
            {
                LogWarn("Attempting to make multiple instances of mod, ignoring");
                return;
            }

            // Set instance for outside use
            Instance = this;

            // Make sure the play mode screen is always unlocked
            Ref.GM.EnablePermadeathMode();

            // Unlock godseeker too because idk why not
            Ref.GM.SetStatusRecordInt("RecBossRushMode", 1);

            Assembly randoDLL = GetType().Assembly;
            
            try
            {
                LanguageStringManager.LoadLanguageXML(
                    randoDLL.GetManifestResourceStream("RandomizerMod.Resources.language.xml"));
            }
            catch (Exception e)
            {
                LogError("Could not process language xml:\n" + e);
            }

            // TODO: insert Data.Setup();

            _logicParseThread = new Thread(() =>
            _LogicManager.ParseXML(randoDLL));
            _logicParseThread.Start();

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
            ModHooks.Instance.SavegameLoadHook += OnLoadGame;

            // Setup preloaded objects
            ObjectCache.GetPrefabs(preloaded);

            _logicParseThread.Join(); // new update -- logic manager is needed to supply start locations to menu
            MenuChanger.EditUI();
        }

        public override List<(string, string)> GetPreloadNames()
        {
            var preloads = new List<(string, string)>
            {
                (SceneNames.Tutorial_01, "_Props/Chest/Item/Shiny Item (1)"),
                (SceneNames.Tutorial_01, "_Enemies/Crawler 1"),
                (SceneNames.Tutorial_01, "_Props/Cave Spikes (1)"),
                (SceneNames.Tutorial_01, "_Markers/Death Respawn Marker"),
                (SceneNames.Tutorial_01, "_Scenery/plat_float_17"),
                (SceneNames.Tutorial_01, "_Props/Tut_tablet_top"),
                (SceneNames.Tutorial_01, "_Props/Geo Rock 1"),
                (SceneNames.Cliffs_02, "Soul Totem 5"),
                (SceneNames.Room_Jinn, "Jinn NPC"),
                (SceneNames.Abyss_19, "Grub Bottle/Grub"),
            };
            if (!globalSettings.ReducePreloads)
            {
                preloads.AddRange(new List<(string, string)>
                {
                    (SceneNames.Abyss_19, "Geo Rock Abyss"),
                    (SceneNames.Ruins2_05, "Geo Rock City 1"),
                    (SceneNames.Deepnest_02, "Geo Rock Deepnest"),
                    (SceneNames.Fungus2_11, "Geo Rock Fung 01"),
                    (SceneNames.Fungus2_11, "Geo Rock Fung 02"),
                    (SceneNames.RestingGrounds_10, "Geo Rock Grave 01"),
                    (SceneNames.RestingGrounds_10, "Geo Rock Grave 02"),
                    (SceneNames.Fungus1_12, "Geo Rock Green Path 01"),
                    (SceneNames.Fungus1_12, "Geo Rock Green Path 02"),
                    (SceneNames.Hive_01, "Geo Rock Hive"),
                    (SceneNames.Mines_20, "Geo Rock Mine (4)"),
                    (SceneNames.Deepnest_East_17, "Geo Rock Outskirts"),
                    (SceneNames.Deepnest_East_17, "Giant Geo Egg")
                });
            }
            return preloads;
        }

        public void HookRandomizer()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            ModHooks.Instance.LanguageGetHook += LanguageStringManager.GetLanguageString;
            
            On.PlayMakerFSM.OnEnable += FixVoidHeart;
            On.GameManager.BeginSceneTransition += EditTransition;
            

            RecentItems.Hook();
            PDHooks.Hook();
            CustomSkills.Hook();
            RandomizerAction.Hook();
            SceneEditor.Hook();

            HookBenchwarp();
        }

        public void UnhookRandomizer()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= HandleSceneChanges;
            ModHooks.Instance.LanguageGetHook -= LanguageStringManager.GetLanguageString;

            On.PlayMakerFSM.OnEnable -= FixVoidHeart;
            On.GameManager.BeginSceneTransition -= EditTransition;
            

            RecentItems.UnHook();
            PDHooks.UnHook();
            CustomSkills.UnHook();
            RandomizerAction.UnHook();
            SceneEditor.UnHook();

            UnHookBenchwarp();
        }

        private void OnLoadGame(int saveSlot)
        {
            if (Settings?.Randomizer ?? false)
            {
                RandomizerMod.Instance.HookRandomizer();
                RandomizerAction.CreateActions(Settings.ItemPlacements, Settings);
            }
        }

        private static Func<
                    (string respawnScene, string respawnMarkerName, int respawnType, int mapZone),
                    (string respawnScene, string respawnMarkerName, int respawnType, int mapZone)
                    >
                    BenchwarpGetStartDef = def => Instance == null || Instance.Settings == null ? def :
                    (Instance.Settings.StartSceneName, Instance.Settings.StartRespawnMarkerName, Instance.Settings.StartRespawnType, Instance.Settings.StartMapZone);

        private void HookBenchwarp()
        {
            try
            {
                FieldInfo field = Type.GetType("Benchwarp.Events, Benchwarp")
                    .GetField("OnGetStartDef", BindingFlags.Public | BindingFlags.Static);

                field.FieldType
                    .GetEvent("Event", BindingFlags.Public | BindingFlags.Instance)
                    .AddEventHandler(field.GetValue(null), BenchwarpGetStartDef);
            }
            catch
            {
                LogWarn("Randomizer was unable to access Benchwarp. Installing the latest version of Benchwarp is strongly advised.");
                return;
            }
        }

        private void UnHookBenchwarp()
        {
            try
            {
                FieldInfo field = Type.GetType("Benchwarp.Events, Benchwarp")
                    .GetField("OnGetStartDef", BindingFlags.Public | BindingFlags.Static);

                field.FieldType
                    .GetEvent("Event", BindingFlags.Public | BindingFlags.Instance)
                    .RemoveEventHandler(field.GetValue(null), BenchwarpGetStartDef);
            }
            catch
            {
                return;
            }
        }

        public static bool LoadComplete()
        {
            return _logicParseThread == null || !_logicParseThread.IsAlive;
        }

        public void StartNewGame()
        {
            if (!Settings.Randomizer)
            {
                return;
            }

            HookRandomizer();

            if (!LoadComplete())
            {
                _logicParseThread.Join();
            }

            RandoLogger.InitializeSpoiler();
            RandoLogger.InitializeCondensedSpoiler();

            try
            {
                _Randomizer.Randomize();

                RandoLogger.UpdateHelperLog();
            }
            catch (Exception e)
            {
                LogError("Error in randomization:\n" + e);
            }

            RandoLogger.InitializeTracker();
        }

        public int MakeAssemblyHash()
        {
            SHA1 sha1 = SHA1.Create();
            FileStream stream = File.OpenRead(Assembly.GetExecutingAssembly().Location);
            byte[] hash = sha1.ComputeHash(stream).ToArray();
            stream.Dispose();
            sha1.Clear();

            unchecked
            {
                int val = 0;
                for (int i = 0; i < hash.Length - 1; i += 4)
                {
                    val = 17 * val + 31 * BitConverter.ToInt32(hash, i);
                }
                return val;
            }
        }

        public override string GetVersion()
        {
            string ver = "3.12";

            ver += $"({Math.Abs(MakeAssemblyHash() % 997)})";

            int minAPI = 53;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            if (apiTooLow)
            {
                return ver + " (Update API)";
            }

            return ver;
        }

        

        private void FixVoidHeart(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            // Normal shade and sibling AI
            if ((self.FsmName == "Control" && self.gameObject.name.StartsWith("Shade Sibling")) || (self.FsmName == "Shade Control" && self.gameObject.name.StartsWith("Hollow Shade")))
            {
                self.FsmVariables.FindFsmBool("Friendly").Value = false;
                self.GetState("Pause").ClearTransitions();
                self.GetState("Pause").AddTransition("FINISHED", "Init");
            }
            // Make Void Heart equippable
            else if (self.FsmName == "UI Charms" && self.gameObject.name == "Charms")
            {
                self.GetState("Equipped?").RemoveTransitionsTo("Black Charm? 2");
                self.GetState("Equipped?").AddTransition("EQUIPPED", "Return Points");
                self.GetState("Set Current Item Num").RemoveTransitionsTo("Black Charm?");
                self.GetState("Set Current Item Num").AddTransition("FINISHED", "Return Points");
            }
        }

        // Will be moved out of RandomizerMod in the future

        public string LastRandomizedEntrance = null;
        public string LastRandomizedExit = null;

        private static void EditTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (PlayerData.instance.bossRushMode && info.SceneName == "GG_Entrance_Cutscene")
            {
                StartSaveChanges.StartDataChanges();
                info.SceneName = PlayerData.instance.respawnScene;
                SceneEditor.ApplySaveDataChanges(info.SceneName, info.EntryGateName ?? string.Empty);
                orig(self, info);
                return;
            }
            if (string.IsNullOrEmpty(info.EntryGateName) || string.IsNullOrEmpty(info.SceneName))
            {
                orig(self, info);
                return;
            }
            if (RandomizerMod.Instance.Settings.RandomizeTransitions)
            {
                TransitionPoint tp = Object.FindObjectsOfType<TransitionPoint>().FirstOrDefault(x => x.entryPoint == info.EntryGateName && x.targetScene == info.SceneName);
                string transitionName = string.Empty;

                if (tp == null)
                {
                    if (self.sceneName == SceneNames.Fungus3_44 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Crossroads_02 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Crossroads_06 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Deepnest_10 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Town && info.SceneName == SceneNames.Room_shop) transitionName = self.sceneName + "[door_sly]";
                    else if (self.sceneName == SceneNames.Town && info.SceneName == SceneNames.Room_Town_Stag_Station) transitionName = self.sceneName + "[door_station]";
                    else if (self.sceneName == SceneNames.Town && info.SceneName == SceneNames.Room_Bretta) transitionName = self.sceneName + "[door_bretta]";
                    else if (self.sceneName == SceneNames.Crossroads_04 && info.SceneName == SceneNames.Room_Charm_Shop) transitionName = self.sceneName + "[door_charmshop]";
                    else if (self.sceneName == SceneNames.Crossroads_04 && info.SceneName == SceneNames.Room_Mender_House) transitionName = self.sceneName + "[door_Mender_House]";
                    else if (self.sceneName == SceneNames.Ruins1_04 && info.SceneName == SceneNames.Room_nailsmith) transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Fungus3_48 && info.SceneName == SceneNames.Room_Queen) transitionName = self.sceneName + "[door1]";
                    else
                    {
                        orig(self, info);
                        return;
                    }
                }
                else
                {
                    string name = tp.name.Split(null).First(); // some transitions have duplicates named left1 (1) and so on

                    if (RandomizerMod.Instance.Settings.RandomizeRooms)
                    {
                        // It's simplest to treat the three transitions connecting Mantis Lords and Mantis Village as one
                        if (self.sceneName == SceneNames.Fungus2_14 && name.StartsWith("bot")) name = "bot3";
                        else if (self.sceneName == SceneNames.Fungus2_15 && name.StartsWith("top")) name = "top3";
                    }

                    transitionName = self.sceneName + "[" + name + "]";
                }

                if (Instance.Settings._transitionPlacements.TryGetValue(transitionName, out string destination))
                {
                    Instance.LastRandomizedEntrance = transitionName;
                    Instance.LastRandomizedExit = destination;

                    try
                    {
                        if (!RandomizerMod.Instance.Settings.CheckTransitionFound(transitionName))
                        {
                            RandomizerMod.Instance.Settings.MarkTransitionFound(transitionName);
                            RandomizerMod.Instance.Settings.MarkTransitionFound(destination);
                            RandoLogger.LogTransitionToTracker(transitionName, destination);
                            // moved UpdateHelperLog to SceneEditor, so it accesses new scene name
                        }
                    }
                    catch (Exception e)
                    {
                        RandomizerMod.Instance.LogError("Error in logging new transition: " + transitionName + "\n" + e);
                    }
                    info.SceneName = _LogicManager.GetTransitionDef(destination).sceneName.Split('-').First();
                    info.EntryGateName = _LogicManager.GetTransitionDef(destination).doorName;
                }
            }
            SceneEditor.ApplySaveDataChanges(info.SceneName, info.EntryGateName);
            orig(self, info);
        }


        private void OnMainMenu(Scene from, Scene to)
        {
            if (SereCore.Ref.GM.GetSceneNameString() != SceneNames.Menu_Title) return;
            // Reset on menu load
            Settings = new SaveSettings();
            RandomizerAction.ClearActions();
            UnhookRandomizer();

            try
            {
                MenuChanger.EditUI();
            }
            catch (Exception e)
            {
                LogError("Error editing menu:\n" + e);
            }
        }
        

        private void HandleSceneChanges(Scene from, Scene to)
        {
            if (SereCore.Ref.GM.IsGameplayScene())
            {
                try
                {
                    // In rare cases, this is called before the previous scene has unloaded
                    // Deleting old randomizer shinies to prevent issues
                    foreach (GameObject g in GameObject.FindObjectsOfType<GameObject>())
                    {
                        if (g.name.Contains("Randomizer Shiny"))
                        {
                            Object.DestroyImmediate(g);
                        }
                    }

                    RandomizerAction.EditShinies();
                }
                catch (Exception e)
                {
                    LogError($"Error applying RandomizerActions to scene {to.name}:\n" + e);
                }
            }

            try
            {
                SceneEditor.SceneChanged(to);
                StartSaveChanges.StartSceneChanges(to);
            }
            catch (Exception e)
            {
                LogError($"Error applying changes to scene {to.name}:\n" + e);
            }
        }
    }
}
