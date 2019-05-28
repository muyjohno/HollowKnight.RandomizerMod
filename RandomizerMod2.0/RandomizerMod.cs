using System;
using Random = System.Random;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using RandomizerMod.Randomization;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod<SaveSettings>
    {
        private static Dictionary<string, Sprite> sprites;
        private static Dictionary<string, string> secondaryBools;

        private static Thread logicParseThread;

        public static RandomizerMod Instance { get; private set; }

        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.OrderingRules",
            "SA1204:StaticElementsMustAppearBeforeInstanceElements",
            Justification = "Initialize is essentially the class constructor")]
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
            GameManager.instance.EnablePermadeathMode();

            // Unlock godseeker too because idk why not
            GameManager.instance.SetStatusRecordInt("RecBossRushMode", 1);
            sprites = new Dictionary<string, Sprite>();

            // Load logo and xml from embedded resources
            Assembly randoDLL = GetType().Assembly;
            foreach (string res in randoDLL.GetManifestResourceNames())
            {
                if (res.EndsWith(".png"))
                {
                    // Read bytes of image
                    Stream imageStream = randoDLL.GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);
                    imageStream.Dispose();

                    // Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer, true);

                    // Create sprite from texture
                    sprites.Add(
                        Path.GetFileNameWithoutExtension(res.Replace("RandomizerMod.Resources.", string.Empty)),
                        Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));

                    LogDebug("Created sprite from embedded image: " + res);
                }
                else if (res.EndsWith("language.xml"))
                {
                    // No sense having the whole init die if this xml is formatted improperly
                    try
                    {
                        LanguageStringManager.LoadLanguageXML(randoDLL.GetManifestResourceStream(res));
                    }
                    catch (Exception e)
                    {
                        LogError("Could not process language xml:\n" + e);
                    }
                }
                else if (res.EndsWith("items.xml"))
                {
                    // Thread the xml parsing because it's kinda slow
                    logicParseThread = new Thread(Randomization.LogicManager.ParseXML);
                    logicParseThread.Start(randoDLL.GetManifestResourceStream(res));
                }
                else
                {
                    Log("Unknown resource " + res);
                }
            }

            // Add hooks
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            ModHooks.Instance.LanguageGetHook += LanguageStringManager.GetLanguageString;
            ModHooks.Instance.GetPlayerIntHook += IntOverride;
            ModHooks.Instance.GetPlayerBoolHook += BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += BoolSetOverride;
            On.PlayMakerFSM.OnEnable += FixVoidHeart;

            Actions.RandomizerAction.Hook();
            BenchHandler.Hook();
            MiscSceneChanges.Hook();

            // Setup preloaded objects
            ObjectCache.GetPrefabs(preloaded[SceneNames.Tutorial_01]);

            // Load fonts
            FontManager.LoadFonts();

            // Some items have two bools for no reason, gotta deal with that
            secondaryBools = new Dictionary<string, string>();

            secondaryBools.Add(nameof(PlayerData.hasDash), nameof(PlayerData.canDash));
            secondaryBools.Add(nameof(PlayerData.hasShadowDash), nameof(PlayerData.canShadowDash));
            secondaryBools.Add(nameof(PlayerData.hasSuperDash), nameof(PlayerData.canSuperDash));
            secondaryBools.Add(nameof(PlayerData.hasWalljump), nameof(PlayerData.canWallJump));

            // Marking unbreakable charms as secondary too to make shade skips viable
            secondaryBools.Add(nameof(PlayerData.gotCharm_23), nameof(PlayerData.fragileHealth_unbreakable));
            secondaryBools.Add(nameof(PlayerData.gotCharm_24), nameof(PlayerData.fragileGreed_unbreakable));
            secondaryBools.Add(nameof(PlayerData.gotCharm_25), nameof(PlayerData.fragileStrength_unbreakable));
        }



        public override List<(string, string)> GetPreloadNames() => new List<(string, string)>()
        {
            (SceneNames.Tutorial_01, "_Props/Chest/Item/Shiny Item (1)"),
            (SceneNames.Tutorial_01, "_Enemies/Crawler 1"),
            (SceneNames.Tutorial_01, "_Props/Cave Spikes (1)")
        };

        public static Sprite GetSprite(string name)
        {
            if (sprites != null && sprites.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }
            return null;
        }

        public static bool LoadComplete()
        {
            return logicParseThread == null || !logicParseThread.IsAlive;
        }

        public void StartNewGame()
        {
            // Charm tutorial popup is annoying, get rid of it
            PlayerData.instance.hasCharm = true;

            //Lantern start for easy mode
            if (!RandomizerMod.Instance.Settings.MiscSkips && !RandomizerMod.Instance.Settings.RandomizeKeys)
            {
                PlayerData.instance.hasLantern = true;
            }
            // Fast boss intros
            PlayerData.instance.unchainedHollowKnight = true;
            PlayerData.instance.encounteredMimicSpider = true;
            PlayerData.instance.infectedKnightEncountered = true;
            PlayerData.instance.mageLordEncountered = true;
            PlayerData.instance.mageLordEncountered_2 = true;

            if (Settings.AllBosses)
            {
                // TODO: Think of a better way to handle Zote
                PlayerData.instance.zoteRescuedBuzzer = true;
                PlayerData.instance.zoteRescuedDeepnest = true;
            }

            if (Settings.Randomizer)
            {
                if (!LoadComplete())
                {
                    logicParseThread.Join();
                }

                try
                {
                    Randomization.Randomizer.Randomize();
                }
                catch (Exception e)
                {
                    LogError("Error in randomization:\n" + e);
                }

                Settings.actions = new List<Actions.RandomizerAction>();
                Settings.actions.AddRange(Randomization.Randomizer.Actions);
            }
        }

        public override string GetVersion()
        {
            string ver = "2.7";
            int minAPI = 49;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            if (apiTooLow)
            {
                return ver + " (Update API)";
            }

            return ver;
        }

        private void UpdateCharmNotches(PlayerData pd, HeroController controller)
        {
            // Update charm notches
            if (Settings.CharmNotch)
            {
                if (pd == null)
                {
                    return;
                }

                pd.CountCharms();
                int charms = pd.charmsOwned;
                int notches = pd.charmSlots;

                if (!pd.salubraNotch1 && charms >= 5)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch1), true);
                    notches++;
                }

                if (!pd.salubraNotch2 && charms >= 10)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch2), true);
                    notches++;
                }

                if (!pd.salubraNotch3 && charms >= 18)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch3), true);
                    notches++;
                }

                if (!pd.salubraNotch4 && charms >= 25)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch4), true);
                    notches++;
                }

                pd.SetInt(nameof(PlayerData.charmSlots), notches);
                GameManager.instance.RefreshOvercharm();
            }
        }

#warning Fix this mess
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "Suppressing to turn into one warning. I'll deal with this mess later")]
        private bool BoolGetOverride(string boolName)
        {
            // Fake spell bools
            if (boolName == "hasVengefulSpirit") return PlayerData.instance.fireballLevel > 0;
            if (boolName == "hasShadeSoul") return PlayerData.instance.fireballLevel > 1;
            if (boolName == "hasDesolateDive") return PlayerData.instance.quakeLevel > 0;
            if (boolName == "hasDescendingDark") return PlayerData.instance.quakeLevel > 1;
            if (boolName == "hasHowlingWraiths") return PlayerData.instance.screamLevel > 0;
            if (boolName == "hasAbyssShriek") return PlayerData.instance.screamLevel > 1;
            
            // This variable is incredibly stubborn, not worth the effort to make it cooperate
            // Just override it completely
            if (boolName == nameof(PlayerData.gotSlyCharm) && Settings.Randomizer) return Settings.SlyCharm;

            if (boolName.StartsWith("RandomizerMod.")) return Settings.GetBool(false, boolName.Substring(14));

            return PlayerData.instance.GetBoolInternal(boolName);
        }

        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "Suppressing to turn into one warning. I'll deal with this mess later")]
        private void BoolSetOverride(string boolName, bool value)
        {
            PlayerData pd = PlayerData.instance;

            // It's just way easier if I can treat spells as bools
            if (boolName == "hasVengefulSpirit" && value && pd.fireballLevel <= 0) pd.SetInt("fireballLevel", 1);
            else if (boolName == "hasVengefulSpirit" && !value) pd.SetInt("fireballLevel", 0);
            else if (boolName == "hasShadeSoul" && value) pd.SetInt("fireballLevel", 2);
            else if (boolName == "hasShadeSoul" && !value && pd.fireballLevel >= 2) pd.SetInt("fireballLevel", 1);
            else if (boolName == "hasDesolateDive" && value && pd.quakeLevel <= 0) pd.SetInt("quakeLevel", 1);
            else if (boolName == "hasDesolateDive" && !value) pd.SetInt("quakeLevel", 0);
            else if (boolName == "hasDescendingDark" && value) pd.SetInt("quakeLevel", 2);
            else if (boolName == "hasDescendingDark" && !value && pd.quakeLevel >= 2) pd.SetInt("quakeLevel", 1);
            else if (boolName == "hasHowlingWraiths" && value && pd.screamLevel <= 0) pd.SetInt("screamLevel", 1);
            else if (boolName == "hasHowlingWraiths" && !value) pd.SetInt("screamLevel", 0);
            else if (boolName == "hasAbyssShriek" && value) pd.SetInt("screamLevel", 2);
            else if (boolName == "hasAbyssShriek" && !value && pd.screamLevel >= 2) pd.SetInt("screamLevel", 1);
            else if (boolName.StartsWith("RandomizerMod."))
            {
                boolName = boolName.Substring(14);
                if (boolName.StartsWith("ShopFireball")) pd.IncrementInt("fireballLevel");
                else if (boolName.StartsWith("ShopQuake")) pd.IncrementInt("quakeLevel");
                else if (boolName.StartsWith("ShopScream")) pd.IncrementInt("screamLevel");
                else if (boolName.StartsWith("ShopDash"))
                {
                    if (pd.hasDash)
                    {
                        pd.SetBool("hasShadowDash", true);
                    }
                    else
                    {
                        pd.SetBool("hasDash", true);
                    }
                }
                else if (boolName.StartsWith("ShopDreamNail"))
                {
                    if (pd.hasDreamNail)
                    {
                        pd.SetBool(nameof(PlayerData.hasDreamGate), true);
                    }
                    else
                    {
                        pd.SetBool(nameof(PlayerData.hasDreamNail), true);
                    }
                }
                else if (boolName.StartsWith("ShopKingsoul") || boolName.StartsWith("QueenFragment") || boolName.StartsWith("VoidHeart"))
                {
                    pd.SetBoolInternal("gotCharm_36", true);
                    if (pd.royalCharmState == 1) pd.SetInt("royalCharmState", 3);
                    else pd.IncrementInt("royalCharmState");
                    if (pd.royalCharmState == 4) pd.SetBoolInternal("gotShadeCharm", true);
                }
                else if (boolName.StartsWith("KingFragment"))
                {
                    pd.SetBoolInternal("gotCharm_36", true);
                    if (pd.royalCharmState == 0) pd.SetInt("royalCharmState", 2);
                    else if (pd.royalCharmState == 1) pd.SetInt("royalCharmState", 3);
                    else pd.IncrementInt("royalCharmState");
                    if (pd.royalCharmState == 4) pd.SetBoolInternal("gotShadeCharm", true);
                }
                else if (boolName.StartsWith("Lurien"))
                {
                    pd.SetBoolInternal("lurienDefeated", true);
                    pd.SetBoolInternal("maskBrokenLurien", true);
                    pd.IncrementInt("guardiansDefeated");
                    if (pd.guardiansDefeated == 1)
                    {
                        pd.SetBoolInternal("hornetFountainEncounter", true);
                        pd.SetBoolInternal("marmOutside", true);
                        pd.SetBoolInternal("crossroadsInfected", true);
                    }
                    if (pd.lurienDefeated && pd.hegemolDefeated && pd.monomonDefeated)
                    {
                        pd.SetBoolInternal("dungDefenderSleeping", true);
                        pd.SetInt("mrMushroomState", 1);
                        pd.IncrementInt("brettaState");
                    }
                }
                else if (boolName.StartsWith("Monomon"))
                {
                    pd.SetBoolInternal("monomonDefeated", true);
                    pd.SetBoolInternal("maskBrokenMonomon", true);
                    pd.IncrementInt("guardiansDefeated");
                    if (pd.guardiansDefeated == 1)
                    {
                        pd.SetBoolInternal("hornetFountainEncounter", true);
                        pd.SetBoolInternal("marmOutside", true);
                        pd.SetBoolInternal("crossroadsInfected", true);
                    }
                    if (pd.lurienDefeated && pd.hegemolDefeated && pd.monomonDefeated)
                    {
                        pd.SetBoolInternal("dungDefenderSleeping", true);
                        pd.SetInt("mrMushroomState", 1);
                        pd.IncrementInt("brettaState");
                    }
                }
                else if (boolName.StartsWith("Herrah"))
                {
                    pd.SetBoolInternal("hegemolDefeated", true);
                    pd.SetBoolInternal("maskBrokenHegemol", true);
                    pd.IncrementInt("guardiansDefeated");
                    if (pd.guardiansDefeated == 1)
                    {
                        pd.SetBoolInternal("hornetFountainEncounter", true);
                        pd.SetBoolInternal("marmOutside", true);
                        pd.SetBoolInternal("crossroadsInfected", true);
                    }
                    if (pd.lurienDefeated && pd.hegemolDefeated && pd.monomonDefeated)
                    {
                        pd.SetBoolInternal("dungDefenderSleeping", true);
                        pd.SetInt("mrMushroomState", 1);
                        pd.IncrementInt("brettaState");
                    }
                }
                else if (boolName.StartsWith("BasinSimpleKey") || boolName.StartsWith("CitySimpleKey") || boolName.StartsWith("SlySimpleKey") || boolName.StartsWith("LurkerSimpleKey"))
                {
                    pd.IncrementInt("simpleKeys");
                }
                else if (boolName.StartsWith("hasWhite") || boolName.StartsWith("hasLove") || boolName.StartsWith("hasSly")) pd.SetBoolInternal(boolName, true);
                else if (boolName.StartsWith("MaskShard"))
                {
                    pd.SetBoolInternal("heartPieceCollected", true);
                    if (PlayerData.instance.heartPieces < 3) GameManager.instance.IncrementPlayerDataInt("heartPieces");
                    else
                    {
                        HeroController.instance.AddToMaxHealth(1);
                        if (PlayerData.instance.maxHealthBase < PlayerData.instance.maxHealthCap) PlayerData.instance.SetIntInternal("heartPieces", 0);
                        PlayMakerFSM.BroadcastEvent("MAX HP UP");
                    }
                }
                else if (boolName.StartsWith("VesselFragment"))
                {
                    pd.SetBoolInternal("vesselFragmentCollected", true);
                    if (PlayerData.instance.vesselFragments < 2) GameManager.instance.IncrementPlayerDataInt("vesselFragments");
                    else
                    {
                        HeroController.instance.AddToMaxMPReserve(33);
                        if (PlayerData.instance.MPReserveMax < PlayerData.instance.MPReserveCap) PlayerData.instance.SetIntInternal("vesselFragments", 0);
                        PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
                    }
                }
                else if (boolName.StartsWith("PaleOre"))
                {
                    pd.IncrementInt("ore");
                }
                Settings.SetBool(value, boolName);
                return;
            }
            // Send the set through to the actual set
            pd.SetBoolInternal(boolName, value);

            // Check if there is a secondary bool for this item
            if (secondaryBools.TryGetValue(boolName, out string secondaryBoolName))
            {
                pd.SetBool(secondaryBoolName, value);
            }

            if (boolName == nameof(PlayerData.hasCyclone) || boolName == nameof(PlayerData.hasUpwardSlash) || boolName == nameof(PlayerData.hasDashSlash))
            {
                // Make nail arts work
                bool hasCyclone = pd.GetBool(nameof(PlayerData.hasCyclone));
                bool hasUpwardSlash = pd.GetBool(nameof(PlayerData.hasUpwardSlash));
                bool hasDashSlash = pd.GetBool(nameof(PlayerData.hasDashSlash));

                pd.SetBool(nameof(PlayerData.hasNailArt), hasCyclone || hasUpwardSlash || hasDashSlash);
                pd.SetBool(nameof(PlayerData.hasAllNailArts), hasCyclone && hasUpwardSlash && hasDashSlash);
            }
            else if (boolName == nameof(PlayerData.hasDreamGate) && value)
            {
                // Make sure the player can actually use dream gate after getting it
                FSMUtility.LocateFSM(HeroController.instance.gameObject, "Dream Nail").FsmVariables.GetFsmBool("Dream Warp Allowed").Value = true;
            }
            else if (boolName == nameof(PlayerData.hasAcidArmour) && value)
            {
                // Gotta update the acid pools after getting this
                PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR");
            }
            else if (boolName.StartsWith("gotCharm_"))
            {
                // Check for Salubra notches if it's a charm
                UpdateCharmNotches(pd, HeroController.instance);
            }
        }

        private int IntOverride(string intName)
        {
            if (intName == "RandomizerMod.Zero")
            {
                return 0;
            }

            return PlayerData.instance.GetIntInternal(intName);
        }

        private void FixVoidHeart(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            // Normal shade and sibling AI
            if ((self.FsmName == "Control" && self.gameObject.name.StartsWith("Shade Sibling")) || (self.FsmName == "Shade Control" && self.gameObject.name.StartsWith("Hollow Shade")))
            {
                self.FsmVariables.FindFsmBool("Friendly").SafeAssign(false);
                self.GetState("Pause").ClearTransitions();
                self.GetState("Pause").AddTransition("FINISHED", "Init");
            }
            // Make Void Heart unequippable
            else if (self.FsmName == "UI Charms" && self.gameObject.name == "Charms")
            {
                self.GetState("Equipped?").RemoveTransitionsTo("Black Charm? 2");
                self.GetState("Equipped?").AddTransition("EQUIPPED", "Return Points");
                self.GetState("Set Current Item Num").RemoveTransitionsTo("Black Charm?");
                self.GetState("Set Current Item Num").AddTransition("FINISHED", "Return Points");
            }
        }

        private void HandleSceneChanges(Scene from, Scene to)
        {
            if (GameManager.instance.GetSceneNameString() == SceneNames.Menu_Title)
            {
                // Reset settings on menu load
                Settings = new SaveSettings();

                try
                {
                    MenuChanger.EditUI();
                }
                catch (Exception e)
                {
                    LogError("Error editing menu:\n" + e);
                }
            }
            else if (GameManager.instance.GetSceneNameString() == SceneNames.End_Credits && Settings != null && Settings.Randomizer && Settings.itemPlacements.Count != 0)
            {
#warning Unfinished functionality here
                /*foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    Object.Destroy(obj);
                }

                GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
                float y = -30;
                foreach (KeyValuePair<string, string> item in Settings.itemPlacements)
                {
                    y -= 1020 / Settings.itemPlacements.Count;
                    CanvasUtil.CreateTextPanel(canvas, item.Key + " - " + item.Value, 16, TextAnchor.UpperLeft, new CanvasUtil.RectData(new Vector2(1920, 50), new Vector2(0, y), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0f, 0f)), FontManager.GetFont("Perpetua"));
                }*/
            }

            if (GameManager.instance.IsGameplayScene())
            {
                try
                {
                    // In rare cases, this is called before the previous scene has unloaded
                    // Deleting old randomizer shinies to prevent issues
                    GameObject oldShiny = GameObject.Find("Randomizer Shiny");
                    if (oldShiny != null)
                    {
                        Object.DestroyImmediate(oldShiny);
                    }

                    EditShinies(to);
                }
                catch (Exception e)
                {
                    LogError($"Error applying RandomizerActions to scene {to.name}:\n" + e);
                }
            }

            try
            {
                RestrictionManager.SceneChanged(to);
                MiscSceneChanges.SceneChanged(to);
            }
            catch (Exception e)
            {
                LogError($"Error applying changes to scene {to.name}:\n" + e);
            }
        }

        private void EditShinies(Scene to)
        {
            string scene = GameManager.instance.GetSceneNameString();

            foreach (Actions.RandomizerAction action in Settings.actions)
            {
                if (action.Type == Actions.RandomizerAction.ActionType.GameObject)
                {
                    try
                    {
                        action.Process(scene, null);
                    }
                    catch (Exception e)
                    {
                        LogError($"Error processing action of type {action.GetType()}:\n{JsonUtility.ToJson(action)}\n{e}");
                    }
                }
            }
        }
    }
}
