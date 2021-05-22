﻿using System;
using RandomizerMod.Extensions;
using SereCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static RandomizerMod.LogHelper;
using Object = UnityEngine.Object;
using Random = System.Random;
using RandomizerMod.Randomization;
using RandomizerMod.RandomizerData;

namespace RandomizerMod
{
    internal static class MenuChanger
    {
        private static readonly Color TRUE_COLOR = Color.Lerp(Color.white, Color.yellow, 0.5f);
        private static readonly Color FALSE_COLOR = Color.grey;
        private static readonly Color LOCKED_TRUE_COLOR = Color.Lerp(Color.grey, Color.yellow, 0.5f);
        private static readonly Color LOCKED_FALSE_COLOR = Color.Lerp(Color.grey, Color.black, 0.5f);

        public static void EditUI()
        {
            // Reset settings
            RandomizerMod.Instance.Settings = new SaveSettings();

            // Fetch data from vanilla screen
            MenuScreen playScreen = SereCore.Ref.UI.playModeMenuScreen;

            playScreen.title.gameObject.transform.localPosition = new Vector3(0, 520.56f);

            Object.Destroy(playScreen.topFleur.gameObject);

            MenuButton classic = (MenuButton)playScreen.defaultHighlight;
            MenuButton steel = (MenuButton)classic.FindSelectableOnDown();
            MenuButton back = (MenuButton)steel.FindSelectableOnDown();

            GameObject parent = steel.transform.parent.gameObject;

            Object.Destroy(parent.GetComponent<VerticalLayoutGroup>());

            // Create new buttons
            MenuButton startRandoBtn = classic.Clone("StartRando", MenuButton.MenuButtonType.Proceed,
                new Vector2(0, 0), "Start Game", "Randomizer", RandomizerMod.GetSprite("UI.logo"));
            
            /*
            MenuButton startNormalBtn = classic.Clone("StartNormal", MenuButton.MenuButtonType.Proceed,
                new Vector2(0, -200), "Start Game", "Non-Randomizer");
            MenuButton startSteelRandoBtn = steel.Clone("StartSteelRando", MenuButton.MenuButtonType.Proceed,
                new Vector2(10000, 10000), "Steel Soul", "Randomizer", RandomizerMod.GetSprite("UI.logo2"));
            MenuButton startSteelNormalBtn = steel.Clone("StartSteelNormal", MenuButton.MenuButtonType.Proceed,
                new Vector2(10000, 10000), "Steel Soul", "Non-Randomizer");
                
            startNormalBtn.transform.localScale = 
                startSteelNormalBtn.transform.localScale =
                    startSteelRandoBtn.transform.localScale = */
            startRandoBtn.transform.localScale = new Vector2(0.75f, 0.75f);
            startRandoBtn.GetComponent<StartGameEventTrigger>().bossRush = true;
            MenuButton backBtn = back.Clone("Back", MenuButton.MenuButtonType.Proceed, new Vector2(0, -100), "Back");


            //RandoMenuItem<string> gameTypeBtn = new RandoMenuItem<string>(back, new Vector2(0, 600), "Game Type", "Normal", "Steel Soul");

            RandoMenuItem<string> presetPoolsBtn = new RandoMenuItem<string>(back, new Vector2(900, 1200), "Preset", "Standard", "Super", "LifeTotems", "Spoiler DAB", "EVERYTHING", "Vanilla", "Custom");
            RandoMenuItem<bool> RandoDreamersBtn = new RandoMenuItem<bool>(back, new Vector2(700, 1120), "Dreamers", true, false);
            RandoMenuItem<bool> RandoSkillsBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 1120), "Skills", true, false);
            RandoMenuItem<bool> RandoCharmsBtn = new RandoMenuItem<bool>(back, new Vector2(700, 1040), "Charms", true, false);
            RandoMenuItem<bool> RandoKeysBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 1040), "Keys", true, false);
            RandoMenuItem<bool> DuplicateBtn = new RandoMenuItem<bool>(back, new Vector2(900, 960), "Duplicate Major Items", true, false);
            RandoMenuItem<bool> RandoMaskBtn = new RandoMenuItem<bool>(back, new Vector2(700, 880), "Mask Shards", true, false);
            RandoMenuItem<bool> RandoVesselBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 880), "Vessel Fragments", true, false);
            RandoMenuItem<bool> RandoOreBtn = new RandoMenuItem<bool>(back, new Vector2(700, 800), "Pale Ore", true, false);
            RandoMenuItem<bool> RandoNotchBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 800), "Charm Notches", true, false);
            RandoMenuItem<bool> RandoGeoChestsBtn = new RandoMenuItem<bool>(back, new Vector2(700, 720), "Geo Chests", true, false);
            RandoMenuItem<bool> RandoRelicsBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 720), "Relics", true, false);
            RandoMenuItem<bool> RandoEggBtn = new RandoMenuItem<bool>(back, new Vector2(700, 640), "Rancid Eggs", true, false);
            RandoMenuItem<bool> RandoStagBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 640), "Stags", true, false);
            RandoMenuItem<bool> RandoMapBtn = new RandoMenuItem<bool>(back, new Vector2(700, 560), "Maps", false, true);
            RandoMenuItem<bool> RandoRootsBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 560), "Whispering Roots", false, true);
            RandoMenuItem<bool> RandoGrubBtn = new RandoMenuItem<bool>(back, new Vector2(700, 480), "Grubs", false, true);
            RandoMenuItem<bool> RandoCocoonsBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 480), "Lifeblood Cocoons", false, true);
            RandoMenuItem<bool> RandoSoulTotemsBtn = new RandoMenuItem<bool>(back, new Vector2(700, 400), "Soul Totems", false, true);
            RandoMenuItem<bool> RandoLoreTabletsBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 400), "Lore Tablets", false, true);
            RandoMenuItem<bool> RandoFlamesBtn = new RandoMenuItem<bool>(back, new Vector2(700, 320), "Grimmkin Flames", false, true);
            RandoMenuItem<bool> RandoGeoRocksBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 320), "Geo Rocks", false, true);
            RandoMenuItem<bool> RandoBossEssenceBtn = new RandoMenuItem<bool>(back, new Vector2(700, 240), "Boss Essence", false, true);
            RandoMenuItem<bool> RandoBossGeoBtn = new RandoMenuItem<bool>(back, new Vector2(1100, 240), "Boss Geo", false, true);

            RandoMenuItem<bool> RandoPalaceBtn = new RandoMenuItem<bool>(back, new Vector2(0, 400), "Palace Totems/Tablets", false, true);

            RandoMenuItem<bool> RandoStartItemsBtn = new RandoMenuItem<bool>(back, new Vector2(900, 80), "Randomize Start Items", false, true);
            RandoMenuItem<string> RandoStartLocationsModeBtn = new RandoMenuItem<string>(back, new Vector2(900, 0), "Start Location Setting", "Select", "Random");
            RandoMenuItem<string> StartLocationsListBtn = new RandoMenuItem<string>(back, new Vector2(900, -80), "Start Location", _LogicManager.StartLocations);

            RandoMenuItem<string> presetSkipsBtn = new RandoMenuItem<string>(back, new Vector2(-900, 1120), "Preset", "Easy", "Medium", "Hard", "Custom");
            RandoMenuItem<bool> mildSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 1040), "Mild Skips", false, true);
            RandoMenuItem<bool> shadeSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 960), "Shade Skips", false, true);
            RandoMenuItem<bool> fireballSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 880), "Fireball Skips", false, true);
            RandoMenuItem<bool> acidSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 800), "Acid Skips", false, true);
            RandoMenuItem<bool> spikeTunnelsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 720), "Spike Tunnels", false, true);
            RandoMenuItem<bool> darkRoomsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 640), "Dark Rooms", false, true);
            RandoMenuItem<bool> spicySkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 560), "Spicy Skips", false, true);

            RandoMenuItem<bool> charmNotchBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 380), "Salubra Notches", true, false);
            RandoMenuItem<bool> preloadsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 300), "Reduce Preloads", true, false);
            preloadsBtn.SetSelection(RandomizerMod.Instance.globalSettings.ReducePreloads);
            RandoMenuItem<bool> EarlyGeoBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 220), "Early Geo", true, false);
            RandoMenuItem<bool> softlockBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 140), "Extra Platforms", true, false);
            RandoMenuItem<bool> recentItemsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 60), "Show Recent Items", true, false);
            recentItemsBtn.SetSelection(RandomizerMod.Instance.globalSettings.RecentItems);
            RandoMenuItem<bool> npcBtn = new RandoMenuItem<bool>(back, new Vector2(-900, -20), "NPC Item Dialogue", true, false);
            npcBtn.SetSelection(RandomizerMod.Instance.globalSettings.NPCItemDialogue);
            RandoMenuItem<bool> jijiBtn = new RandoMenuItem<bool>(back, new Vector2(-900, -100), "Jiji Hints", false, true);

            RandoMenuItem<string> modeBtn = new RandoMenuItem<string>(back, new Vector2(0, 1040), "Mode", "Item Randomizer", "Item + Area Randomizer", "Item + Connected-Area Room Randomizer", "Item + Room Randomizer");
            RandoMenuItem<string> cursedBtn = new RandoMenuItem<string>(back, new Vector2(0, 960), "Cursed", "no", "noo", "noooo", "noooooooo", "noooooooooooooooo", "Oh yeah", "Just focus");
            RandoMenuItem<bool> splitCloakBtn = new RandoMenuItem<bool>(back, new Vector2(-250, 880), "Split Cloak", false, true);
            RandoMenuItem<bool> splitClawBtn = new RandoMenuItem<bool>(back, new Vector2(250, 880), "Split Claw", false, true);
            RandoMenuItem<bool> cursedNailBtn = new RandoMenuItem<bool>(back, new Vector2(0, 800), "Cursed Nail", false, true);
            RandoMenuItem<bool> RandoSpoilerBtn = new RandoMenuItem<bool>(back, new Vector2(0, 0), "Create Spoiler Log", true, false);

            // Create seed entry field
            GameObject seedGameObject = back.Clone("Seed", MenuButton.MenuButtonType.Activate, new Vector2(0, 1130),
                "Click to type a custom seed").gameObject;
            Object.DestroyImmediate(seedGameObject.GetComponent<MenuButton>());
            Object.DestroyImmediate(seedGameObject.GetComponent<EventTrigger>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<AutoLocalizeTextUI>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<FixVerticalAlign>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<ContentSizeFitter>());

            RectTransform seedRect = seedGameObject.transform.Find("Text").GetComponent<RectTransform>();
            seedRect.anchorMin = seedRect.anchorMax = new Vector2(0.5f, 0.5f);
            seedRect.sizeDelta = new Vector2(337, 63.2f);

            InputField customSeedInput = seedGameObject.AddComponent<InputField>();
            customSeedInput.transform.localPosition = new Vector3(0, 1240);
            customSeedInput.textComponent = seedGameObject.transform.Find("Text").GetComponent<Text>();

            RandomizerMod.Instance.Settings.Seed = new Random().Next(999999999);
            customSeedInput.text = RandomizerMod.Instance.Settings.Seed.ToString();

            customSeedInput.caretColor = Color.white;
            customSeedInput.contentType = InputField.ContentType.IntegerNumber;
            customSeedInput.onEndEdit.AddListener(ParseSeedInput);
            customSeedInput.navigation = Navigation.defaultNavigation;
            customSeedInput.caretWidth = 8;
            customSeedInput.characterLimit = 9;

            customSeedInput.colors = new ColorBlock
            {
                highlightedColor = Color.yellow,
                pressedColor = Color.red,
                disabledColor = Color.black,
                normalColor = Color.white,
                colorMultiplier = 2f
            };

            // Create some labels
            CreateLabel(back, new Vector2(-900, 1210), "Required Skips");
            CreateLabel(back, new Vector2(-900, 460), "Quality of Life");
            CreateLabel(back, new Vector2(900, 1290), "Item Randomization");
            CreateLabel(back, new Vector2(900, 160), "Start Settings");
            CreateLabel(back, new Vector2(0, 200), "Use of Benchwarp mod may be required");
            CreateLabel(back, new Vector2(0, 1300), "Seed:");

            // We don't need these old buttons anymore
            Object.Destroy(classic.gameObject);
            Object.Destroy(steel.gameObject);
            Object.Destroy(parent.FindGameObjectInChildren("GGButton"));
            Object.Destroy(back.gameObject);

            // Gotta put something here, we destroyed the old default
            playScreen.defaultHighlight = startRandoBtn;

            // Apply navigation info (up, right, down, left)
            //startNormalBtn.SetNavigation(gameTypeBtn.Button, presetPoolsBtn.Button, backBtn, presetSkipsBtn.Button);
            startRandoBtn.SetNavigation(modeBtn.Button, presetPoolsBtn.Button, backBtn, presetSkipsBtn.Button);
            //startSteelNormalBtn.SetNavigation(gameTypeBtn.Button, presetPoolsBtn.Button, backBtn, presetSkipsBtn.Button);
            //startSteelRandoBtn.SetNavigation(modeBtn.Button, presetPoolsBtn.Button, gameTypeBtn.Button, presetSkipsBtn.Button);
            modeBtn.Button.SetNavigation(backBtn, modeBtn.Button, startRandoBtn, modeBtn.Button);
            //gameTypeBtn.Button.SetNavigation(startRandoBtn, presetPoolsBtn.Button, startRandoBtn, presetSkipsBtn.Button);
            backBtn.SetNavigation(startRandoBtn, backBtn, modeBtn.Button, backBtn);
            RandoSpoilerBtn.Button.SetNavigation(RandoRelicsBtn.Button, RandoSpoilerBtn.Button, presetPoolsBtn.Button, startRandoBtn);

            presetSkipsBtn.Button.SetNavigation(jijiBtn.Button, startRandoBtn, shadeSkipsBtn.Button, presetSkipsBtn.Button);
            mildSkipsBtn.Button.SetNavigation(presetSkipsBtn.Button, startRandoBtn, mildSkipsBtn.Button, mildSkipsBtn.Button);
            shadeSkipsBtn.Button.SetNavigation(mildSkipsBtn.Button, startRandoBtn, fireballSkipsBtn.Button, shadeSkipsBtn.Button);
            fireballSkipsBtn.Button.SetNavigation(shadeSkipsBtn.Button, startRandoBtn, acidSkipsBtn.Button, fireballSkipsBtn.Button);
            acidSkipsBtn.Button.SetNavigation(fireballSkipsBtn.Button, startRandoBtn, spikeTunnelsBtn.Button, acidSkipsBtn.Button);
            spikeTunnelsBtn.Button.SetNavigation(acidSkipsBtn.Button, startRandoBtn, darkRoomsBtn.Button, spikeTunnelsBtn.Button);
            darkRoomsBtn.Button.SetNavigation(spikeTunnelsBtn.Button, startRandoBtn, spicySkipsBtn.Button, darkRoomsBtn.Button);
            spicySkipsBtn.Button.SetNavigation(darkRoomsBtn.Button, startRandoBtn, charmNotchBtn.Button, spicySkipsBtn.Button);

            charmNotchBtn.Button.SetNavigation(spicySkipsBtn.Button, startRandoBtn, preloadsBtn.Button, charmNotchBtn.Button);
            preloadsBtn.Button.SetNavigation(charmNotchBtn.Button, startRandoBtn, EarlyGeoBtn.Button, preloadsBtn.Button);
            EarlyGeoBtn.Button.SetNavigation(preloadsBtn.Button, startRandoBtn, softlockBtn.Button, EarlyGeoBtn.Button);
            softlockBtn.Button.SetNavigation(EarlyGeoBtn.Button, startRandoBtn, npcBtn.Button, softlockBtn.Button);
            npcBtn.Button.SetNavigation(softlockBtn.Button, startRandoBtn, jijiBtn.Button, npcBtn.Button);
            jijiBtn.Button.SetNavigation(npcBtn.Button, startRandoBtn, presetSkipsBtn.Button, jijiBtn.Button);

            presetPoolsBtn.Button.SetNavigation(RandoSpoilerBtn.Button, presetPoolsBtn.Button, RandoDreamersBtn.Button, startRandoBtn);
            RandoDreamersBtn.Button.SetNavigation(presetPoolsBtn.Button, RandoSkillsBtn.Button, RandoCharmsBtn.Button, startRandoBtn);
            RandoSkillsBtn.Button.SetNavigation(presetPoolsBtn.Button, RandoSkillsBtn.Button, RandoKeysBtn.Button, RandoDreamersBtn.Button);
            RandoCharmsBtn.Button.SetNavigation(RandoDreamersBtn.Button, RandoKeysBtn.Button, DuplicateBtn.Button, startRandoBtn);
            RandoKeysBtn.Button.SetNavigation(RandoSkillsBtn.Button, RandoKeysBtn.Button, DuplicateBtn.Button, RandoCharmsBtn.Button);
            DuplicateBtn.Button.SetNavigation(RandoCharmsBtn.Button, DuplicateBtn.Button, RandoMaskBtn.Button, startRandoBtn);
            RandoMaskBtn.Button.SetNavigation(DuplicateBtn.Button, RandoVesselBtn.Button, RandoOreBtn.Button, startRandoBtn);
            RandoVesselBtn.Button.SetNavigation(DuplicateBtn.Button, RandoVesselBtn.Button, RandoNotchBtn.Button, RandoMaskBtn.Button);
            RandoOreBtn.Button.SetNavigation(RandoMaskBtn.Button, RandoNotchBtn.Button, RandoGeoChestsBtn.Button, startRandoBtn);
            RandoNotchBtn.Button.SetNavigation(RandoVesselBtn.Button, RandoNotchBtn.Button, RandoRelicsBtn.Button, RandoOreBtn.Button);
            RandoGeoChestsBtn.Button.SetNavigation(RandoOreBtn.Button, RandoRelicsBtn.Button, RandoEggBtn.Button, startRandoBtn);
            RandoRelicsBtn.Button.SetNavigation(RandoNotchBtn.Button, RandoRelicsBtn.Button, RandoStagBtn.Button, RandoGeoChestsBtn.Button);
            RandoEggBtn.Button.SetNavigation(RandoGeoChestsBtn.Button, RandoStagBtn.Button, RandoMapBtn.Button, startRandoBtn);
            RandoStagBtn.Button.SetNavigation(RandoRelicsBtn.Button, RandoStagBtn.Button, RandoRootsBtn.Button, RandoEggBtn.Button);
            RandoMapBtn.Button.SetNavigation(RandoEggBtn.Button, RandoRootsBtn.Button, RandoGrubBtn.Button, startRandoBtn);
            RandoRootsBtn.Button.SetNavigation(RandoStagBtn.Button, RandoRootsBtn.Button, RandoCocoonsBtn.Button, RandoMapBtn.Button);
            RandoGrubBtn.Button.SetNavigation(RandoMapBtn.Button, RandoCocoonsBtn.Button, RandoSoulTotemsBtn.Button, startRandoBtn);
            RandoCocoonsBtn.Button.SetNavigation(RandoRootsBtn.Button, RandoCocoonsBtn.Button, RandoLoreTabletsBtn.Button, RandoGrubBtn.Button);
            RandoSoulTotemsBtn.Button.SetNavigation(RandoGrubBtn.Button, RandoLoreTabletsBtn.Button, RandoFlamesBtn.Button, startRandoBtn);
            RandoLoreTabletsBtn.Button.SetNavigation(RandoCocoonsBtn.Button, RandoLoreTabletsBtn.Button, RandoGeoRocksBtn.Button, RandoSoulTotemsBtn.Button);
            RandoFlamesBtn.Button.SetNavigation(RandoSoulTotemsBtn.Button, RandoGeoRocksBtn.Button, RandoBossEssenceBtn.Button, startRandoBtn);
            RandoGeoRocksBtn.Button.SetNavigation(RandoLoreTabletsBtn.Button, RandoGeoRocksBtn.Button, RandoBossGeoBtn.Button, RandoFlamesBtn.Button);
            RandoBossEssenceBtn.Button.SetNavigation(RandoFlamesBtn.Button, RandoBossGeoBtn.Button, RandoStartItemsBtn.Button, RandoBossEssenceBtn.Button);
            RandoBossGeoBtn.Button.SetNavigation(RandoGeoRocksBtn.Button, RandoBossGeoBtn.Button, RandoStartItemsBtn.Button, RandoBossEssenceBtn.Button);
            
            RandoStartItemsBtn.Button.SetNavigation(RandoBossEssenceBtn.Button, RandoStartItemsBtn.Button, RandoStartLocationsModeBtn.Button, startRandoBtn);
            RandoStartLocationsModeBtn.Button.SetNavigation(RandoStartItemsBtn.Button, RandoStartLocationsModeBtn.Button, StartLocationsListBtn.Button, startRandoBtn);
            StartLocationsListBtn.Button.SetNavigation(RandoStartLocationsModeBtn.Button, RandoStartLocationsModeBtn.Button, StartLocationsListBtn.Button, startRandoBtn);

            void UpdateSkipsButtons(RandoMenuItem<string> item)
            {
                switch (item.CurrentSelection)
                {
                    case "Easy":
                        SetShadeSkips(false);
                        mildSkipsBtn.SetSelection(false);
                        acidSkipsBtn.SetSelection(false);
                        spikeTunnelsBtn.SetSelection(false);
                        spicySkipsBtn.SetSelection(false);
                        fireballSkipsBtn.SetSelection(false);
                        darkRoomsBtn.SetSelection(false);
                        break;
                    case "Medium":
                        SetShadeSkips(true);
                        mildSkipsBtn.SetSelection(true);
                        acidSkipsBtn.SetSelection(false);
                        spikeTunnelsBtn.SetSelection(false);
                        spicySkipsBtn.SetSelection(false);
                        fireballSkipsBtn.SetSelection(false);
                        darkRoomsBtn.SetSelection(false);
                        break;
                    case "Hard":
                        SetShadeSkips(true);
                        mildSkipsBtn.SetSelection(true);
                        acidSkipsBtn.SetSelection(true);
                        spikeTunnelsBtn.SetSelection(true);
                        spicySkipsBtn.SetSelection(true);
                        fireballSkipsBtn.SetSelection(true);
                        darkRoomsBtn.SetSelection(true);
                        break;
                    case "Custom":
                        item.SetSelection("Easy");
                        goto case "Easy";

                    default:
                        LogWarn("Unknown value in preset button: " + item.CurrentSelection);
                        break;
                }
            }
            void UpdatePoolPreset(RandoMenuItem<string> item)
            {
                switch (item.CurrentSelection)
                {
                    case "Standard":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(true);
                        RandoRelicsBtn.SetSelection(true);
                        RandoMapBtn.SetSelection(false);
                        RandoStagBtn.SetSelection(true);
                        RandoGrubBtn.SetSelection(false);
                        RandoRootsBtn.SetSelection(false);
                        RandoGeoRocksBtn.SetSelection(false);
                        RandoCocoonsBtn.SetSelection(false);
                        RandoSoulTotemsBtn.SetSelection(false);
                        RandoPalaceBtn.SetSelection(false);
                        RandoLoreTabletsBtn.SetSelection(false);
                        RandoFlamesBtn.SetSelection(false);
                        RandoBossEssenceBtn.SetSelection(false);
                        RandoBossGeoBtn.SetSelection(false);
                        break;
                    case "Super":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(true);
                        RandoRelicsBtn.SetSelection(true);
                        RandoMapBtn.SetSelection(true);
                        RandoStagBtn.SetSelection(true);
                        RandoGrubBtn.SetSelection(true);
                        RandoRootsBtn.SetSelection(true);
                        RandoGeoRocksBtn.SetSelection(false);
                        RandoCocoonsBtn.SetSelection(false);
                        RandoSoulTotemsBtn.SetSelection(false);
                        RandoPalaceBtn.SetSelection(false);
                        RandoLoreTabletsBtn.SetSelection(false);
                        RandoFlamesBtn.SetSelection(false);
                        RandoBossEssenceBtn.SetSelection(false);
                        RandoBossGeoBtn.SetSelection(false);
                        break;
                    case "LifeTotems":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(true);
                        RandoRelicsBtn.SetSelection(true);
                        RandoMapBtn.SetSelection(false);
                        RandoStagBtn.SetSelection(true);
                        RandoGrubBtn.SetSelection(false);
                        RandoRootsBtn.SetSelection(false);
                        RandoGeoRocksBtn.SetSelection(false);
                        RandoCocoonsBtn.SetSelection(true);
                        RandoSoulTotemsBtn.SetSelection(true);
                        RandoPalaceBtn.SetSelection(true);
                        RandoLoreTabletsBtn.SetSelection(false);
                        RandoFlamesBtn.SetSelection(false);
                        RandoBossEssenceBtn.SetSelection(false);
                        RandoBossGeoBtn.SetSelection(false);
                        break;
                    case "Spoiler DAB":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(true);
                        RandoRelicsBtn.SetSelection(true);
                        RandoMapBtn.SetSelection(true);
                        RandoStagBtn.SetSelection(true);
                        RandoGrubBtn.SetSelection(false);
                        RandoRootsBtn.SetSelection(true);
                        RandoGeoRocksBtn.SetSelection(false);
                        RandoCocoonsBtn.SetSelection(true);
                        RandoSoulTotemsBtn.SetSelection(true);
                        RandoPalaceBtn.SetSelection(false);
                        RandoLoreTabletsBtn.SetSelection(false);
                        RandoFlamesBtn.SetSelection(false);
                        RandoBossEssenceBtn.SetSelection(false);
                        RandoBossGeoBtn.SetSelection(false);
                        break;
                    case "EVERYTHING":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(true);
                        RandoRelicsBtn.SetSelection(true);
                        RandoMapBtn.SetSelection(true);
                        RandoStagBtn.SetSelection(true);
                        RandoGrubBtn.SetSelection(true);
                        RandoRootsBtn.SetSelection(true);
                        RandoGeoRocksBtn.SetSelection(true);
                        RandoCocoonsBtn.SetSelection(true);
                        RandoSoulTotemsBtn.SetSelection(true);
                        RandoPalaceBtn.SetSelection(true);
                        RandoLoreTabletsBtn.SetSelection(true);
                        RandoFlamesBtn.SetSelection(true);
                        RandoBossEssenceBtn.SetSelection(true);
                        RandoBossGeoBtn.SetSelection(true);
                        break;
                    case "Vanilla":
                        RandoDreamersBtn.SetSelection(false);
                        RandoSkillsBtn.SetSelection(false);
                        RandoCharmsBtn.SetSelection(false);
                        RandoKeysBtn.SetSelection(false);
                        RandoGeoChestsBtn.SetSelection(false);
                        RandoMaskBtn.SetSelection(false);
                        RandoVesselBtn.SetSelection(false);
                        RandoOreBtn.SetSelection(false);
                        RandoNotchBtn.SetSelection(false);
                        RandoEggBtn.SetSelection(false);
                        RandoRelicsBtn.SetSelection(false);
                        RandoMapBtn.SetSelection(false);
                        RandoStagBtn.SetSelection(false);
                        RandoGrubBtn.SetSelection(false);
                        RandoRootsBtn.SetSelection(false);
                        RandoGeoRocksBtn.SetSelection(false);
                        RandoCocoonsBtn.SetSelection(false);
                        RandoSoulTotemsBtn.SetSelection(false);
                        RandoPalaceBtn.SetSelection(false);
                        RandoLoreTabletsBtn.SetSelection(false);
                        RandoFlamesBtn.SetSelection(false);
                        RandoBossEssenceBtn.SetSelection(false);
                        RandoBossGeoBtn.SetSelection(false);
                        break;
                    case "Custom":
                        item.SetSelection("Standard");
                        goto case "Standard";
                }
            }

            void UpdateStartLocationColor()
            {
                if (RandoStartLocationsModeBtn.CurrentSelection == "Random")
                {
                    StartLocationsListBtn.SetSelection("King's Pass");
                    StartLocationsListBtn.Lock();
                    StartLocationsListBtn.SetColor(LOCKED_FALSE_COLOR);
                    return;
                }
                else StartLocationsListBtn.Unlock();

                // cf. TestStartLocation in PreRandomizer. Note that color is checked in StartGame to determine if a selected start was valid
                if (Data.GetStartDef(StartLocationsListBtn.CurrentSelection) is StartDef startDef)
                {
                    if (RandoStartItemsBtn.CurrentSelection)
                    {
                        StartLocationsListBtn.SetColor(Color.white);
                    }
                    else if (modeBtn.CurrentSelection.EndsWith("Room Randomizer"))
                    {
                        if (startDef.roomSafe)
                        {
                            StartLocationsListBtn.SetColor(Color.white);
                        }
                        else StartLocationsListBtn.SetColor(Color.red);
                    }
                    else if (modeBtn.CurrentSelection.EndsWith("Area Randomizer"))
                    {
                        if (startDef.areaSafe)
                        {
                            StartLocationsListBtn.SetColor(Color.white);
                        }
                        else StartLocationsListBtn.SetColor(Color.red);
                    }
                    else if (startDef.itemSafe)
                    {
                        if (startDef.sceneName == "Mines_35" && !EarlyGeoBtn.CurrentSelection)
                        {
                            StartLocationsListBtn.SetColor(Color.red);
                        }
                        else StartLocationsListBtn.SetColor(Color.white);
                    }
                    else StartLocationsListBtn.SetColor(Color.red);
                }
            }

            void HandleProgressionLock()
            {
                if (RandoStartItemsBtn.CurrentSelection)
                {
                    RandoDreamersBtn.SetSelection(true);
                    RandoSkillsBtn.SetSelection(true);
                    RandoCharmsBtn.SetSelection(true);
                    RandoKeysBtn.SetSelection(true);
                    RandoDreamersBtn.Lock();
                    RandoSkillsBtn.Lock();
                    RandoCharmsBtn.Lock();
                    RandoKeysBtn.Lock();
                }
                else
                {
                    RandoDreamersBtn.Unlock();
                    RandoSkillsBtn.Unlock();
                    RandoCharmsBtn.Unlock();
                    RandoKeysBtn.Unlock();
                }
            }
            HandleProgressionLock(); // call it because duplicates are on by default

            void SetShadeSkips(bool enabled)
            {
                if (enabled)
                {
                    //gameTypeBtn.SetSelection("Normal");
                    //SwitchGameType(false);
                }

                shadeSkipsBtn.SetSelection(enabled);
            }

            void SkipsSettingChanged(RandoMenuItem<bool> item)
            {
                presetSkipsBtn.SetSelection("Custom");

            }

            void PoolSettingChanged(RandoMenuItem<bool> item)
            {
                presetPoolsBtn.SetSelection("Custom");
            }

            void RecentItemsSettingChanged(RandoMenuItem<bool> item)
            {
                RandomizerMod.Instance.globalSettings.RecentItems = recentItemsBtn.CurrentSelection;
            }
            void NPCSettingChanged(RandoMenuItem<bool> item)
            {
                RandomizerMod.Instance.globalSettings.NPCItemDialogue = npcBtn.CurrentSelection;
            }

            void PreloadsSettingChanged(RandoMenuItem<bool> item)
            {
                RandomizerMod.Instance.globalSettings.ReducePreloads = preloadsBtn.CurrentSelection;
            }
            modeBtn.Changed += s => HandleProgressionLock();

            presetSkipsBtn.Changed += UpdateSkipsButtons;
            presetPoolsBtn.Changed += UpdatePoolPreset;

            mildSkipsBtn.Changed += SkipsSettingChanged;
            shadeSkipsBtn.Changed += SkipsSettingChanged;
            shadeSkipsBtn.Changed += SaveShadeVal;
            acidSkipsBtn.Changed += SkipsSettingChanged;
            spikeTunnelsBtn.Changed += SkipsSettingChanged;
            spicySkipsBtn.Changed += SkipsSettingChanged;
            fireballSkipsBtn.Changed += SkipsSettingChanged;
            darkRoomsBtn.Changed += SkipsSettingChanged;

            RandoDreamersBtn.Changed += PoolSettingChanged;
            RandoSkillsBtn.Changed += PoolSettingChanged;
            RandoCharmsBtn.Changed += PoolSettingChanged;
            RandoKeysBtn.Changed += PoolSettingChanged;
            RandoGeoChestsBtn.Changed += PoolSettingChanged;
            RandoGeoRocksBtn.Changed += PoolSettingChanged;
            RandoSoulTotemsBtn.Changed += PoolSettingChanged;

            RandoPalaceBtn.Changed += PoolSettingChanged;
            RandoLoreTabletsBtn.Changed += PoolSettingChanged;

            RandoMaskBtn.Changed += PoolSettingChanged;
            RandoVesselBtn.Changed += PoolSettingChanged;
            RandoOreBtn.Changed += PoolSettingChanged;
            RandoNotchBtn.Changed += PoolSettingChanged;
            RandoEggBtn.Changed += PoolSettingChanged;
            RandoRelicsBtn.Changed += PoolSettingChanged;
            RandoStagBtn.Changed += PoolSettingChanged;
            RandoMapBtn.Changed += PoolSettingChanged;
            RandoGrubBtn.Changed += PoolSettingChanged;
            RandoRootsBtn.Changed += PoolSettingChanged;
            RandoCocoonsBtn.Changed += PoolSettingChanged;
            RandoFlamesBtn.Changed += PoolSettingChanged;
            DuplicateBtn.Changed += s => HandleProgressionLock();

            RandoStartItemsBtn.Changed += (RandoMenuItem<bool> Item) => UpdateStartLocationColor();
            RandoStartItemsBtn.Changed += s => HandleProgressionLock();
            RandoStartLocationsModeBtn.Changed += (RandoMenuItem<string> Item) => UpdateStartLocationColor();
            StartLocationsListBtn.Changed += (RandoMenuItem<string> Item) => UpdateStartLocationColor();
            modeBtn.Changed += (RandoMenuItem<string> Item) => UpdateStartLocationColor();
            recentItemsBtn.Changed += RecentItemsSettingChanged;
            npcBtn.Changed += NPCSettingChanged;
            preloadsBtn.Changed += PreloadsSettingChanged;

            // Setup game type button changes
            void SaveShadeVal(RandoMenuItem<bool> item)
            {
                SetShadeSkips(shadeSkipsBtn.CurrentSelection);
            }

            /*void SwitchGameType(bool steelMode)
            {
                if (!steelMode)
                {
                    // Normal mode
                    startRandoBtn.transform.localPosition = new Vector2(0, 200);
                    startNormalBtn.transform.localPosition = new Vector2(0, -200);
                    startSteelRandoBtn.transform.localPosition = new Vector2(10000, 10000);
                    startSteelNormalBtn.transform.localPosition = new Vector2(10000, 10000);

                    //backBtn.SetNavigation(startNormalBtn, startNormalBtn, modeBtn.Button, startRandoBtn);
                   //magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startNormalBtn, lemmBtn.Button);
                    //lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startRandoBtn, allSkillsBtn.Button);
                }
                else
                {
                    // Steel Soul mode
                    startRandoBtn.transform.localPosition = new Vector2(10000, 10000);
                    startNormalBtn.transform.localPosition = new Vector2(10000, 10000);
                    startSteelRandoBtn.transform.localPosition = new Vector2(0, 200);
                    startSteelNormalBtn.transform.localPosition = new Vector2(0, -200);

                    SetShadeSkips(false);

                    //backBtn.SetNavigation(startSteelNormalBtn, startSteelNormalBtn, modeBtn.Button, startSteelRandoBtn);
                    //magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startSteelNormalBtn, lemmBtn.Button);
                    //lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startSteelRandoBtn, allSkillsBtn.Button);
                }
            }

            gameTypeBtn.Button.AddEvent(EventTriggerType.Submit,
                garbage => SwitchGameType(gameTypeBtn.CurrentSelection != "Normal"));
                */

            // Setup start game button events
            void StartGame(bool rando)
            {
                RandomizerMod.Instance.Settings.CharmNotch = charmNotchBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.Grubfather = true;
                RandomizerMod.Instance.Settings.JinnSellAll = true;
                RandomizerMod.Instance.Settings.EarlyGeo = EarlyGeoBtn.CurrentSelection;


                if (rando)
                {
                    //RandomizerMod.Instance.Settings.Jiji = jijiBtn.CurrentSelection;
                    //RandomizerMod.Instance.Settings.Quirrel = false;
                    //RandomizerMod.Instance.Settings.ItemDepthHints = false;
                    RandomizerMod.Instance.Settings.NPCItemDialogue = npcBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.ExtraPlatforms = softlockBtn.CurrentSelection;

                    RandomizerMod.Instance.Settings.RandomizeDreamers = RandoDreamersBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeSkills = RandoSkillsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeCharms = RandoCharmsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeKeys = RandoKeysBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeGeoChests = RandoGeoChestsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeMaskShards = RandoMaskBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeVesselFragments = RandoVesselBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizePaleOre = RandoOreBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeCharmNotches = RandoNotchBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeRancidEggs = RandoEggBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeRelics = RandoRelicsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeMaps = RandoMapBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeStags = RandoStagBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeGrubs = RandoGrubBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeLifebloodCocoons = RandoCocoonsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeWhisperingRoots = RandoRootsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeRocks = RandoGeoRocksBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeSoulTotems = RandoSoulTotemsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeLoreTablets = RandoLoreTabletsBtn.CurrentSelection;                    
                    RandomizerMod.Instance.Settings.RandomizeGrimmkinFlames = RandoFlamesBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeBossEssence = RandoBossEssenceBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeBossGeo = RandoBossGeoBtn.CurrentSelection;

                    RandomizerMod.Instance.Settings.RandomizePalaceTotems = RandoPalaceBtn.CurrentSelection && 
                        (RandoSoulTotemsBtn.CurrentSelection || !RandoLoreTabletsBtn.CurrentSelection);
                    RandomizerMod.Instance.Settings.RandomizePalaceTablets = RandoPalaceBtn.CurrentSelection &&
                        (!RandoSoulTotemsBtn.CurrentSelection || RandoLoreTabletsBtn.CurrentSelection);

                    RandomizerMod.Instance.Settings.DuplicateMajorItems = DuplicateBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.CreateSpoilerLog = RandoSpoilerBtn.CurrentSelection;

                    Ref.CURSE.RemoveSpellUpgrades = Ref.CURSE.ReplaceJunkWithOneGeo = Ref.CURSE.LongerProgressionChains
                        = cursedBtn.CurrentSelection.StartsWith("O");
                    Ref.CURSE.RandomizeFocus = cursedBtn.CurrentSelection.StartsWith("O") || cursedBtn.CurrentSelection.StartsWith("J");
                    RandomizerMod.Instance.Settings.RandomizeCloakPieces = splitCloakBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeClawPieces = splitClawBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.CursedNail = cursedNailBtn.CurrentSelection;

                    RandomizerMod.Instance.Settings.Randomizer = rando;
                    RandomizerMod.Instance.Settings.RandomizeAreas = modeBtn.CurrentSelection.EndsWith("Area Randomizer");
                    RandomizerMod.Instance.Settings.RandomizeRooms = modeBtn.CurrentSelection.EndsWith("Room Randomizer");
                    RandomizerMod.Instance.Settings.ConnectAreas = modeBtn.CurrentSelection.StartsWith("Item + Connected-Area");

                    Ref.SKIP.MildSkips = mildSkipsBtn.CurrentSelection;
                    Ref.SKIP.ShadeSkips = shadeSkipsBtn.CurrentSelection;
                    Ref.SKIP.FireballSkips = fireballSkipsBtn.CurrentSelection;
                    Ref.SKIP.AcidSkips = acidSkipsBtn.CurrentSelection;
                    Ref.SKIP.SpikeTunnels = spikeTunnelsBtn.CurrentSelection;
                    Ref.SKIP.DarkRooms = darkRoomsBtn.CurrentSelection;
                    Ref.SKIP.SpicySkips = spicySkipsBtn.CurrentSelection;

                    RandomizerMod.Instance.Settings.RandomizeStartItems = RandoStartItemsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeStartLocation = RandoStartLocationsModeBtn.CurrentSelection == "Random";
                    RandomizerMod.Instance.Settings.StartName = StartLocationsListBtn.GetColor() == Color.red ? "King's Pass" : StartLocationsListBtn.CurrentSelection;
                }

                RandomizerMod.Instance.StartNewGame();
            }

            //startNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            startRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
            //startSteelNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            //startSteelRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
        }

        private static void ParseSeedInput(string input)
        {
            if (int.TryParse(input, out int newSeed))
            {
                RandomizerMod.Instance.Settings.Seed = newSeed;
            }
            else
            {
                LogWarn($"Seed input \"{input}\" could not be parsed to an integer");
            }
        }

        private static void CreateLabel(MenuButton baseObj, Vector2 position, string text)
        {
            GameObject label = baseObj.Clone(text + "Label", MenuButton.MenuButtonType.Activate, position, text)
                .gameObject;
            Object.Destroy(label.GetComponent<EventTrigger>());
            Object.Destroy(label.GetComponent<MenuButton>());
        }

        private class RandoMenuItem<T> where T : IEquatable<T>
        {
            public delegate void RandoMenuItemChanged(RandoMenuItem<T> item);

            private readonly FixVerticalAlign _align;
            private readonly T[] _selections;
            private readonly Text _text;
            private int _currentSelection;
            private bool _locked = false;

            public RandoMenuItem(MenuButton baseObj, Vector2 position, string name, params T[] values)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                if (baseObj == null)
                {
                    throw new ArgumentNullException(nameof(baseObj));
                }

                if (values == null || values.Length == 0)
                {
                    throw new ArgumentNullException(nameof(values));
                }

                _selections = values;
                Name = name;

                Button = baseObj.Clone(name + "Button", MenuButton.MenuButtonType.Activate, position, string.Empty);

                _text = Button.transform.Find("Text").GetComponent<Text>();
                _text.fontSize = 36;
                _align = Button.gameObject.GetComponentInChildren<FixVerticalAlign>(true);

                Button.ClearEvents();
                Button.AddEvent(EventTriggerType.Submit, GotoNext);

                RefreshText();
            }

            public T CurrentSelection => _selections[_currentSelection];

            public MenuButton Button { get; }

            public string Name { get; }

            public event RandoMenuItemChanged Changed
            {
                add => ChangedInternal += value;
                remove => ChangedInternal -= value;
            }

            private event RandoMenuItemChanged ChangedInternal;

            public void SetSelection(T obj)
            {
                if (_locked) return;

                for (int i = 0; i < _selections.Length; i++)
                {
                    if (_selections[i].Equals(obj))
                    {
                        _currentSelection = i;
                        break;
                    }
                }

                RefreshText(false);
            }

            private void GotoNext(BaseEventData data = null)
            {
                if (_locked) return;

                _currentSelection++;
                if (_currentSelection >= _selections.Length)
                {
                    _currentSelection = 0;
                }

                RefreshText();
            }

            private void GotoPrev(BaseEventData data = null)
            {
                if (_locked) return;

                _currentSelection--;
                if (_currentSelection < 0)
                {
                    _currentSelection = _selections.Length - 1;
                }

                RefreshText();
            }

            private void RefreshText(bool invokeEvent = true)
            {
                if (typeof(T) == typeof(bool))
                {
                    _text.text = Name;
                }
                else
                {
                    _text.text = Name + ": " + _selections[_currentSelection];
                }

                _align.AlignText();
                SetColor();

                if (invokeEvent)
                {
                    ChangedInternal?.Invoke(this);
                }
            }

            internal void SetColor(Color? c = null)
            {
                if (c is Color forceColor)
                {
                    _text.color = forceColor;
                    return;
                }

                if (!(_selections[_currentSelection] is bool value))
                {
                    if (_locked)
                    {
                        _text.color = LOCKED_FALSE_COLOR;
                    }
                    else
                    {
                        _text.color = Color.white;
                    }
                    return;
                }

                if (!_locked && value)
                {
                    _text.color = TRUE_COLOR;
                }
                else if (!_locked && !value)
                {
                    _text.color = FALSE_COLOR;
                }
                else if (_locked && value)
                {
                    _text.color = LOCKED_TRUE_COLOR;
                }
                else if (_locked && value)
                {
                    _text.color = LOCKED_FALSE_COLOR;
                }
                else
                {
                    _text.color = Color.red;
                }
            }
            
            internal Color GetColor()
            {
                return _text.color;
            }

            internal void Lock()
            {
                _locked = true;
                SetColor();
            }

            internal void Unlock()
            {
                _locked = false;
                SetColor();
            }
        }
    }
}
