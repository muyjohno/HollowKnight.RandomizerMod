using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using RandomizerMod.Randomization;
using static RandomizerMod.RandoLogger;
using static RandomizerMod.LogHelper;
using UnityEngine;
using RandomizerMod.RandomizerData;

namespace RandomizerMod
{
    // WORK IN PROGRESS
    public static class GiveItemActions
    {
        public delegate ItemDef OverrideGiveItemHandler(ItemDef item, LocationDef location);
        public delegate void GiveItemListener(ItemDef item, LocationDef location);

        /// <summary>
        /// Event which gives access to initial item, location, prior to resolving additives, invoking override, etc.
        /// </summary>
        public static event GiveItemListener BeforeGiveItem;
        /// <summary>
        /// Event which gives access to final item and location immediately prior to giving the item.
        /// </summary>
        public static event GiveItemListener OnGiveItem;
        /// <summary>
        /// Event which gives access to final item and location after given.
        /// </summary>
        public static event GiveItemListener AfterGiveItem;
        /// <summary>
        /// Event which allows subscriber to override the given item. Called sequentially.
        /// </summary>
        public static event OverrideGiveItemHandler OverrideGiveItem
        {
            add => overrideSubscribers.Add(value);
            remove => overrideSubscribers.Remove(value);
        }
        private static List<OverrideGiveItemHandler> overrideSubscribers = new List<OverrideGiveItemHandler>();
        /// <summary>
        /// Event which allows subscriber to override duplicates of unique items. Called sequentially.
        /// </summary>
        public static event OverrideGiveItemHandler ResolveDuplicateGiveItem
        {
            add => resolveDuplicateSubscribers.Add(value);
            remove => resolveDuplicateSubscribers.Remove(value);
        }
        private static List<OverrideGiveItemHandler> resolveDuplicateSubscribers = new List<OverrideGiveItemHandler>();



        public static void ShowEffectiveItemPopup(string item)
        {
            ReqDef def = _LogicManager.GetItemDef(RandomizerMod.Instance.Settings.GetEffectiveItem(item));
            ShowItemPopup(def.nameKey, def.shopSpriteKey);
        }

        private static void ShowItemPopup(string nameKey, string spriteName)
        {
            GameObject popup = ObjectCache.RelicGetMsg;
            popup.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = LanguageStringManager.GetLanguageString(nameKey, "UI");
            popup.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = Sprites.GetSprite(spriteName);
            popup.SetActive(true);
        }

        public static void Give(ItemDef item, LocationDef location)
        {
            BeforeGiveItem?.Invoke(item, location);
            foreach (var @override in overrideSubscribers) item = @override(item, location);

            // TODO: Determine if item is duplicate and invoke event

            OnGiveItem?.Invoke(item, location);
            GiveImmediate(item);
            AfterGiveItem?.Invoke(item, location);
        }

        public static void GiveImmediate(ItemDef item)
        {
            switch (item.action)
            {
                default:
                case GiveAction.Bool:
                    PlayerData.instance.SetBool(item.fieldName, true);
                    break;

                case GiveAction.Int:
                    PlayerData.instance.IncrementInt(item.fieldName);
                    if (item.fieldName == nameof(PlayerData.flamesCollected))
                    {
                        Ref.SD.Completion.TotalFlamesCollected += 1;
                    }
                    break;

                case GiveAction.Charm:
                    PlayerData.instance.SetBool(nameof(PlayerData.hasCharm), true);
                    PlayerData.instance.SetBool(item.fieldName, true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.charmsOwned));
                    break;

                case GiveAction.EquippedCharm:
                    PlayerData.instance.SetBool(nameof(PlayerData.hasCharm), true);
                    PlayerData.instance.SetBool(item.fieldName, true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.charmsOwned));
                    PlayerData.instance.SetBool(item.equipBoolName, true);
                    PlayerData.instance.EquipCharm(item.charmNum);

                    PlayerData.instance.CalculateNotchesUsed();
                    if (PlayerData.instance.GetInt(nameof(PlayerData.charmSlotsFilled)) > PlayerData.instance.GetInt(nameof(PlayerData.charmSlots)))
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.overcharmed), true);
                    }
                    break;

                // TODO: Implement new additive system
                /*
                case GiveAction.Additive:
                    string[] additiveItems = LogicManager.GetAdditiveItems(LogicManager.AdditiveItemNames.First(s => LogicManager.GetAdditiveItems(s).Contains(item)));
                    int additiveCount = RandomizerMod.Instance.Settings.GetAdditiveCount(item);
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(additiveItems[Math.Min(additiveCount, additiveItems.Length - 1)]).boolName, true);
                    break;
                */

                case GiveAction.AddGeo:
                    HeroController.instance.AddGeo(item.amount);
                    break;

                // Disabled because it's more convenient to do this from the fsm. Use GiveAction.None for geo spawns.
                case GiveAction.SpawnGeo:
                    LogError("Tried to spawn geo from GiveItem.");
                    throw new NotImplementedException();

                case GiveAction.AddSoul:
                    HeroController.instance.AddMPCharge(200);
                    break;

                case GiveAction.Lore:
                    // TODO: intercept lore tablet at shop before this point
                    //if (LogicManager.ShopNames.Contains(location)) break;
                    AudioSource.PlayClipAtPoint(ObjectCache.LoreSound,
                        new Vector3(
                            Camera.main.transform.position.x - 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    AudioSource.PlayClipAtPoint(ObjectCache.LoreSound,
                        new Vector3(
                            Camera.main.transform.position.x + 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    break;

                case GiveAction.Map:
                    PlayerData.instance.SetBool(nameof(PlayerData.hasMap), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.openedMapperShop), true);
                    PlayerData.instance.SetBool(item.fieldName, true);
                    break;

                case GiveAction.Stag:
                    PlayerData.instance.SetBool(item.fieldName, true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.stationsOpened));
                    break;

                case GiveAction.DirtmouthStag:
                    PlayerData.instance.SetBool(nameof(PlayerData.openedTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.openedTownBuilding), true);
                    break;

                case GiveAction.Grub:
                    PlayerData.instance.IncrementInt(nameof(PlayerData.grubsCollected));
                    int clipIndex = new System.Random().Next(2);
                    AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex],
                        new Vector3(
                            Camera.main.transform.position.x - 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex],
                        new Vector3(
                            Camera.main.transform.position.x + 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    break;

                case GiveAction.Essence:
                    PlayerData.instance.IntAdd(nameof(PlayerData.dreamOrbs), item.amount);
                    EventRegister.SendEvent("DREAM ORB COLLECT");
                    break;

                case GiveAction.MaskShard:
                    PlayerData.instance.SetBool(nameof(PlayerData.heartPieceCollected), true);
                    {
                        int inc = item.amount > 1 ? item.amount : 1;
                        int bse = PlayerData.instance.GetInt(nameof(PlayerData.heartPieces)) + inc;
                        for (; bse > 3; bse -= 4)
                        {
                            HeroController.instance.AddToMaxHealth(1);
                            PlayMakerFSM.BroadcastEvent("MAX HP UP");
                            PlayMakerFSM.BroadcastEvent("HERO HEALED FULL");
                        }
                        switch (bse)
                        {
                            case 0 when PlayerData.instance.GetInt(nameof(PlayerData.maxHealthBase)) == PlayerData.instance.GetInt(nameof(PlayerData.maxHealthCap)):
                                PlayerData.instance.SetInt(nameof(PlayerData.heartPieces), 4);
                                break;
                            default:
                                PlayerData.instance.SetInt(nameof(PlayerData.heartPieces), bse);
                                break;
                        }
                    }
                    break;

                case GiveAction.VesselFragment:
                    PlayerData.instance.SetBool(nameof(PlayerData.vesselFragmentCollected), true);
                    {
                        int inc = item.amount > 1 ? item.amount : 1;
                        int bse = PlayerData.instance.GetInt(nameof(PlayerData.vesselFragments)) + inc;
                        for (; bse > 2; bse -= 3)
                        {
                            HeroController.instance.AddToMaxMPReserve(33);
                            PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
                        }
                        switch (bse)
                        {
                            case 0 when PlayerData.instance.GetInt(nameof(PlayerData.MPReserveMax)) == PlayerData.instance.GetInt(nameof(PlayerData.MPReserveCap)):
                                PlayerData.instance.SetInt(nameof(PlayerData.vesselFragments), 3);
                                break;
                            default:
                                PlayerData.instance.SetInt(nameof(PlayerData.vesselFragments), bse);
                                break;
                        }
                    }
                    break;

                case GiveAction.WanderersJournal:
                    PlayerData.instance.SetBool(nameof(PlayerData.foundTrinket1), true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.trinket1));
                    break;

                case GiveAction.HallownestSeal:
                    PlayerData.instance.SetBool(nameof(PlayerData.foundTrinket2), true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.trinket2));
                    break;

                case GiveAction.KingsIdol:
                    PlayerData.instance.SetBool(nameof(PlayerData.foundTrinket3), true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.trinket3));
                    break;

                case GiveAction.ArcaneEgg:
                    PlayerData.instance.SetBool(nameof(PlayerData.foundTrinket4), true);
                    PlayerData.instance.IncrementInt(nameof(PlayerData.trinket4));
                    break;

                case GiveAction.Dreamer:
                    switch (item.name)
                    {
                        case "Lurien":
                            PlayerData.instance.SetBool(nameof(PlayerData.lurienDefeated), true);
                            PlayerData.instance.SetBool(nameof(PlayerData.maskBrokenLurien), true);
                            break;
                        case "Monomon":
                            PlayerData.instance.SetBool(nameof(PlayerData.monomonDefeated), true);
                            PlayerData.instance.SetBool(nameof(PlayerData.maskBrokenMonomon), true);
                            break;
                        case "Herrah":
                            PlayerData.instance.SetBool(nameof(PlayerData.hegemolDefeated), true);
                            PlayerData.instance.SetBool(nameof(PlayerData.maskBrokenHegemol), true);
                            break;
                    }
                    if (PlayerData.instance.GetInt(nameof(PlayerData.guardiansDefeated)) == 0)
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.hornetFountainEncounter), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.marmOutside), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.crossroadsInfected), true);
                    }
                    if (PlayerData.instance.GetInt(nameof(PlayerData.guardiansDefeated)) == 2)
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.dungDefenderSleeping), true);
                        PlayerData.instance.brettaState++;
                        PlayerData.instance.mrMushroomState++;
                        PlayerData.instance.corniferAtHome = true;
                        PlayerData.instance.metIselda = true;
                        PlayerData.instance.corn_cityLeft = true;
                        PlayerData.instance.corn_abyssLeft = true;
                        PlayerData.instance.corn_cliffsLeft = true;
                        PlayerData.instance.corn_crossroadsLeft = true;
                        PlayerData.instance.corn_deepnestLeft = true;
                        PlayerData.instance.corn_fogCanyonLeft = true;
                        PlayerData.instance.corn_fungalWastesLeft = true;
                        PlayerData.instance.corn_greenpathLeft = true;
                        PlayerData.instance.corn_minesLeft = true;
                        PlayerData.instance.corn_outskirtsLeft = true;
                        PlayerData.instance.corn_royalGardensLeft = true;
                        PlayerData.instance.corn_waterwaysLeft = true;
                    }
                    if (PlayerData.instance.guardiansDefeated < 3)
                    {
                        PlayerData.instance.guardiansDefeated++;
                    }
                    break;

                case GiveAction.Kingsoul:
                    if (PlayerData.instance.royalCharmState < 4)
                    {
                        PlayerData.instance.IncrementInt(nameof(PlayerData.royalCharmState));
                    }
                    switch (PlayerData.instance.royalCharmState)
                    {
                        case 1:
                            PlayerData.instance.SetBool(nameof(PlayerData.gotCharm_36), true);
                            PlayerData.instance.IncrementInt(nameof(PlayerData.charmsOwned));
                            break;
                        case 2:
                            PlayerData.instance.IncrementInt(nameof(PlayerData.royalCharmState));
                            break;
                        case 4:
                            PlayerData.instance.gotShadeCharm = true;
                            PlayerData.instance.charmCost_36 = 0;
                            PlayerData.instance.equippedCharm_36 = true;
                            if (!PlayerData.instance.equippedCharms.Contains(36)) PlayerData.instance.equippedCharms.Add(36);
                            break;
                    }
                    break;

                case GiveAction.Grimmchild:
                    PlayerData.instance.SetBool(nameof(PlayerData.instance.gotCharm_40), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.nightmareLanternAppeared), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.nightmareLanternLit), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.troupeInTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.divineInTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.metGrimm), true);
                    PlayerData.instance.SetInt(nameof(PlayerData.flamesRequired), 3);
                    if (RandomizerMod.Instance.Settings.RandomizeGrimmkinFlames)
                    {
                        PlayerData.instance.SetInt(nameof(PlayerData.grimmChildLevel), 1);
                    }
                    else
                    {
                        // Skip first two collection quests
                        PlayerData.instance.SetInt(nameof(PlayerData.flamesCollected), 3);
                        PlayerData.instance.SetBool(nameof(PlayerData.killedFlameBearerSmall), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.killedFlameBearerMed), true);
                        PlayerData.instance.SetInt(nameof(PlayerData.killsFlameBearerSmall), 3);
                        PlayerData.instance.SetInt(nameof(PlayerData.killsFlameBearerMed), 3);
                        PlayerData.instance.SetInt(nameof(PlayerData.grimmChildLevel), 2);
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Mines_10",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_28",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Fungus1_10",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_06",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_East_03",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                    }

                    break;

                case GiveAction.SettingsBool:
                    // TODO: Delete
                    RandomizerMod.Instance.Settings.SetBool(true, item.fieldName);
                    break;

                case GiveAction.None:
                    break;

                case GiveAction.Lifeblood:
                    int n = item.amount;
                    for (int i = 0; i < n; i++)
                    {
                        EventRegister.SendEvent("ADD BLUE HEALTH");
                    }
                    break;
            }
        }

        public static void GiveItem(GiveAction action, string item, string location, int geo = 0)
        {
            LogItemToTracker(item, location);
            RandomizerMod.Instance.Settings.MarkItemFound(item);
            RandomizerMod.Instance.Settings.MarkLocationFound(location);
            UpdateHelperLog();

            item = _LogicManager.RemoveDuplicateSuffix(item);

            if (RandomizerMod.Instance.globalSettings.RecentItems)
            {
                RecentItems.AddItem(item, location, showArea: true);
            }

            switch (action)
            {
                default:
                case GiveAction.Bool:
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(item).boolName, true);
                    break;

                case GiveAction.Int:
                    {
                        string intName = _LogicManager.GetItemDef(item).intName;
                    }
                    PlayerData.instance.IncrementInt(_LogicManager.GetItemDef(item).intName);
                    if (_LogicManager.GetItemDef(item).intName == nameof(PlayerData.instance.flamesCollected))
                    {
                        RandomizerMod.Instance.Settings.TotalFlamesCollected += 1;
                    }
                    break;

                case GiveAction.Charm:
                    PlayerData.instance.hasCharm = true;
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(item).boolName, true);
                    PlayerData.instance.charmsOwned++;
                    break;

                case GiveAction.EquippedCharm:
                    PlayerData.instance.hasCharm = true;
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(item).boolName, true);
                    PlayerData.instance.charmsOwned++;
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(item).equipBoolName, true);
                    PlayerData.instance.EquipCharm(_LogicManager.GetItemDef(item).charmNum);

                    PlayerData.instance.CalculateNotchesUsed();
                    if (PlayerData.instance.charmSlotsFilled > PlayerData.instance.charmSlots)
                    {
                        PlayerData.instance.overcharmed = true;
                    }
                    break;

                case GiveAction.Additive:
                    string[] additiveItems = _LogicManager.GetAdditiveItems(_LogicManager.AdditiveItemNames.First(s => _LogicManager.GetAdditiveItems(s).Contains(item)));
                    int additiveCount = RandomizerMod.Instance.Settings.GetAdditiveCount(item);
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(additiveItems[Math.Min(additiveCount, additiveItems.Length - 1)]).boolName, true);
                    break;

                case GiveAction.AddGeo:
                    if (geo > 0) HeroController.instance.AddGeo(geo);
                    else
                    {
                        HeroController.instance.AddGeo(_LogicManager.GetItemDef(item).geo);
                    }
                    
                    break;

                // Disabled because it's more convenient to do this from the fsm. Use GiveAction.None for geo spawns.
                case GiveAction.SpawnGeo:
                    RandomizerMod.Instance.LogError("Tried to spawn geo from GiveItem.");
                    throw new NotImplementedException();

                case GiveAction.AddSoul:
                    HeroController.instance.AddMPCharge(200);
                    break;

                case GiveAction.Lore:
                    if (_LogicManager.ShopNames.Contains(location)) break;
                    AudioSource.PlayClipAtPoint(ObjectCache.LoreSound,
                        new Vector3(
                            Camera.main.transform.position.x - 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    AudioSource.PlayClipAtPoint(ObjectCache.LoreSound,
                        new Vector3(
                            Camera.main.transform.position.x + 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    break;

                case GiveAction.Map:
                    PlayerData.instance.hasMap = true;
                    PlayerData.instance.openedMapperShop = true;
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(item).boolName, true);
                    break;

                case GiveAction.Stag:
                    PlayerData.instance.SetBool(_LogicManager.GetItemDef(item).boolName, true);
                    PlayerData.instance.stationsOpened++;
                    break;

                case GiveAction.DirtmouthStag:
                    PlayerData.instance.SetBool(nameof(PlayerData.openedTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.openedTownBuilding), true);
                    break;

                case GiveAction.Grub:
                    PlayerData.instance.grubsCollected++;
                    int clipIndex = new System.Random().Next(2);
                    AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex],
                        new Vector3(
                            Camera.main.transform.position.x - 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex],
                        new Vector3(
                            Camera.main.transform.position.x + 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    break;

                case GiveAction.Essence:
                    PlayerData.instance.IntAdd(nameof(PlayerData.dreamOrbs), _LogicManager.GetItemDef(item).geo);
                    EventRegister.SendEvent("DREAM ORB COLLECT");
                    break;

                case GiveAction.MaskShard:
                    PlayerData.instance.heartPieceCollected = true;
                    if (PlayerData.instance.heartPieces < 3)
                    {
                        PlayerData.instance.heartPieces++;
                    }
                    else
                    {
                        HeroController.instance.AddToMaxHealth(1);
                        PlayMakerFSM.BroadcastEvent("MAX HP UP");
                        PlayMakerFSM.BroadcastEvent("HERO HEALED FULL");
                        if (PlayerData.instance.maxHealthBase < PlayerData.instance.maxHealthCap)
                        {
                            PlayerData.instance.heartPieces = 0;
                        }
                    }
                    break;

                case GiveAction.VesselFragment:
                    PlayerData.instance.vesselFragmentCollected = true;
                    if (PlayerData.instance.vesselFragments < 2)
                    {
                        GameManager.instance.IncrementPlayerDataInt("vesselFragments");
                    }
                    else
                    {
                        HeroController.instance.AddToMaxMPReserve(33);
                        PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
                        if (PlayerData.instance.MPReserveMax < PlayerData.instance.MPReserveCap)
                        {
                            PlayerData.instance.vesselFragments = 0;
                        }
                    }
                    break;

                case GiveAction.WanderersJournal:
                    PlayerData.instance.foundTrinket1 = true;
                    PlayerData.instance.trinket1++;
                    break;

                case GiveAction.HallownestSeal:
                    PlayerData.instance.foundTrinket2 = true;
                    PlayerData.instance.trinket2++;
                    break;

                case GiveAction.KingsIdol:
                    PlayerData.instance.foundTrinket3 = true;
                    PlayerData.instance.trinket3++;
                    break;

                case GiveAction.ArcaneEgg:
                    PlayerData.instance.foundTrinket4 = true;
                    PlayerData.instance.trinket4++;
                    break;

                case GiveAction.Dreamer:
                    switch (item)
                    {
                        case "Lurien":
                            if (PlayerData.instance.lurienDefeated) break;
                            PlayerData.instance.lurienDefeated = true;
                            PlayerData.instance.maskBrokenLurien = true;
                            break;
                        case "Monomon":
                            if (PlayerData.instance.monomonDefeated) break;
                            PlayerData.instance.monomonDefeated = true;
                            PlayerData.instance.maskBrokenMonomon = true;
                            break;
                        case "Herrah":
                            if (PlayerData.instance.hegemolDefeated) break;
                            PlayerData.instance.hegemolDefeated = true;
                            PlayerData.instance.maskBrokenHegemol = true;
                            break;
                    }
                    if (PlayerData.instance.guardiansDefeated == 0)
                    {
                        PlayerData.instance.hornetFountainEncounter = true;
                        PlayerData.instance.marmOutside = true;
                        PlayerData.instance.crossroadsInfected = true;
                    }
                    if (PlayerData.instance.guardiansDefeated == 2)
                    {
                        PlayerData.instance.dungDefenderSleeping = true;
                        PlayerData.instance.brettaState++;
                        PlayerData.instance.mrMushroomState++;
                        PlayerData.instance.corniferAtHome = true;
                        PlayerData.instance.metIselda = true;
                        PlayerData.instance.corn_cityLeft = true;
                        PlayerData.instance.corn_abyssLeft = true;
                        PlayerData.instance.corn_cliffsLeft = true;
                        PlayerData.instance.corn_crossroadsLeft = true;
                        PlayerData.instance.corn_deepnestLeft = true;
                        PlayerData.instance.corn_fogCanyonLeft = true;
                        PlayerData.instance.corn_fungalWastesLeft = true;
                        PlayerData.instance.corn_greenpathLeft = true;
                        PlayerData.instance.corn_minesLeft = true;
                        PlayerData.instance.corn_outskirtsLeft = true;
                        PlayerData.instance.corn_royalGardensLeft = true;
                        PlayerData.instance.corn_waterwaysLeft = true;
                    }
                    if (PlayerData.instance.guardiansDefeated < 3)
                    {
                        PlayerData.instance.guardiansDefeated++;
                    }
                    break;

                case GiveAction.Kingsoul:
                    if (PlayerData.instance.royalCharmState < 4)
                    {
                        PlayerData.instance.royalCharmState++;
                    }
                    switch (PlayerData.instance.royalCharmState)
                    {
                        case 1:
                            PlayerData.instance.gotCharm_36 = true;
                            PlayerData.instance.charmsOwned++;
                            break;
                        case 2:
                            PlayerData.instance.royalCharmState++;
                            break;
                        case 4:
                            PlayerData.instance.gotShadeCharm = true;
                            PlayerData.instance.charmCost_36 = 0;
                            PlayerData.instance.equippedCharm_36 = true;
                            if (!PlayerData.instance.equippedCharms.Contains(36)) PlayerData.instance.equippedCharms.Add(36);
                            break;
                    }
                    break;

                case GiveAction.Grimmchild:
                    PlayerData.instance.SetBool(nameof(PlayerData.instance.gotCharm_40), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.nightmareLanternAppeared), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.nightmareLanternLit), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.troupeInTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.divineInTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.metGrimm), true);
                    PlayerData.instance.SetInt(nameof(PlayerData.flamesRequired), 3);
                    if (RandomizerMod.Instance.Settings.RandomizeGrimmkinFlames)
                    {
                        PlayerData.instance.SetInt(nameof(PlayerData.grimmChildLevel), 1);
                    }
                    else
                    {
                        // Skip first two collection quests
                        PlayerData.instance.SetInt(nameof(PlayerData.flamesCollected), 3);
                        PlayerData.instance.SetBool(nameof(PlayerData.killedFlameBearerSmall), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.killedFlameBearerMed), true);
                        PlayerData.instance.SetInt(nameof(PlayerData.killsFlameBearerSmall), 3);
                        PlayerData.instance.SetInt(nameof(PlayerData.killsFlameBearerMed), 3);
                        PlayerData.instance.SetInt(nameof(PlayerData.grimmChildLevel), 2);
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Mines_10",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_28",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Fungus1_10",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_06",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                        GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_East_03",
                            id = "Flamebearer Spawn",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    
                    break;

                case GiveAction.SettingsBool:
                    RandomizerMod.Instance.Settings.SetBool(true, _LogicManager.GetItemDef(item).boolName);
                    break;

                case GiveAction.None:
                    break;
                
                case GiveAction.Lifeblood:
                    int n = _LogicManager.GetItemDef(item).lifeblood;
                    for (int i = 0; i < n; i++)
                    {
                        EventRegister.SendEvent("ADD BLUE HEALTH");
                    }
                    break;
            }

            // With Cursed Nail active, drop the vine platform so they can escape from thorns without softlocking
            // Break the Thorns Vine here; this works whether or not the item is a shiny.
            if (location == "Thorns_of_Agony" && RandomizerMod.Instance.Settings.CursedNail && RandomizerMod.Instance.Settings.ExtraPlatforms)
            {
                if (GameObject.Find("Vine") is GameObject vine)
                {
                    VinePlatformCut vinecut = vine.GetComponent<VinePlatformCut>();
                    bool activated = ReflectionHelper.GetAttr<VinePlatformCut, bool>(vinecut, "activated");
                    if (!activated) vinecut.Cut();
                }
            }

            // additive, kingsoul, bool type items can all have additive counts
            if (_LogicManager.AdditiveItemSets.Any(set => set.Contains(item)))
            {
                RandomizerMod.Instance.Settings.IncrementAdditiveCount(item);
            }
        }
    }
}
