using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerMod.Randomization;
using SeanprCore;
using UnityEngine;
using static RandomizerMod.LogHelper;
using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    public abstract class RandomizerAction
    {
        public enum ActionType
        {
            GameObject,
            PlayMakerFSM
        }

        private static readonly List<RandomizerAction> Actions = new List<RandomizerAction>();
        public static Dictionary<string, string> AdditiveBoolNames = new Dictionary<string, string>(); // item name, additive bool name
        public static Dictionary<(string, string), string> ShopItemBoolNames = new Dictionary<(string, string), string>(); // (item name, shop name), shop item bool name

        public abstract ActionType Type { get; }

        public static void ClearActions()
        {
            Actions.Clear();
        }

        public static void CreateActions((string, string)[] items, bool fromDeserialize = false)
        {
            ClearActions();
            Dictionary<string, int> additiveCounts = null;
            ShopItemBoolNames = new Dictionary<(string, string), string>();
            AdditiveBoolNames = new Dictionary<string, string>();

            int newShinies = 0;
            string[] shopNames = LogicManager.ShopNames;

            if (!fromDeserialize)   // Settings aren't available from AfterDeserialize, so these are done in SaveSettings instead...
            {                       // This is useless since these dictionaries are empty, but this is more clear.
                // best place to handle reassigning random essence/grub costs
                foreach (var pair in RandomizerMod.Instance.Settings.VariableCosts)
                {
                    ReqDef def = LogicManager.GetItemDef(pair.Item1);
                    def.cost = pair.Item2;
                    LogicManager.EditItemDef(pair.Item1, def);
                }

                // reassign shop costs
                foreach (var pair in RandomizerMod.Instance.Settings.ShopCosts)
                {
                    ReqDef def = LogicManager.GetItemDef(pair.Item1);
                    def.shopCost = pair.Item2;
                    LogicManager.EditItemDef(pair.Item1, def);
                }
            }

            // Loop non-shop items
            foreach ((string newItemName, string location) in items.Where(item => !shopNames.Contains(item.Item2)))
            {
                ReqDef oldItem = LogicManager.GetItemDef(location);
                ReqDef newItem = LogicManager.GetItemDef(newItemName);

                if (oldItem.replace)
                {
                    Actions.Add(new ReplaceObjectWithShiny(oldItem.sceneName, oldItem.objectName, "Randomizer Shiny"));
                    oldItem.objectName = "Randomizer Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }
                else if (oldItem.newShiny)
                {
                    string newShinyName = "New Shiny";
                    if (location == "Void_Heart" || location == "Lurien" || location == "Monomon" || location == "Herrah") { } // Give these items a name we can safely refer to in miscscenechanges
                    else
                    {
                        newShinyName = "New Shiny " + newShinies++; // Give the other items a name which safely increments for grub/essence rooms
                    }
                    Actions.Add(new CreateNewShiny(oldItem.sceneName, oldItem.x, oldItem.y, newShinyName));
                    oldItem.objectName = newShinyName;
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }
                else if (oldItem.type == ItemType.Geo && newItem.type != ItemType.Geo)
                {
                    Actions.Add(new AddShinyToChest(oldItem.sceneName, oldItem.objectName, oldItem.fsmName,
                        "Randomizer Chest Shiny"));
                    oldItem.objectName = "Randomizer Chest Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }

                string randomizerBoolName = GetAdditiveBoolName(newItemName, ref additiveCounts);
                bool playerdata = false;
                if (string.IsNullOrEmpty(randomizerBoolName))
                {
                    randomizerBoolName = newItem.boolName;
                    playerdata = newItem.type != ItemType.Geo;
                }

                // Dream nail needs a special case
                if (oldItem.boolName == "hasDreamNail")
                {
                    Actions.Add(new ChangeBoolTest("RestingGrounds_04", "Binding Shield Activate", "FSM", "Check",
                        randomizerBoolName, playerdata));
                    Actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Plaque Inspect",
                        "Conversation Control", "End", randomizerBoolName, playerdata));
                    Actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Scene 2", "Control", "Init",
                        randomizerBoolName, playerdata));
                    Actions.Add(new ChangeBoolTest("RestingGrounds_04", "PreDreamnail", "FSM", "Check",
                        randomizerBoolName, playerdata));
                    Actions.Add(new ChangeBoolTest("RestingGrounds_04", "PostDreamnail", "FSM", "Check",
                        randomizerBoolName, playerdata));
                }

                switch (newItem.type)
                {
                    // make a shiny with no cutscene
                    case ItemType.Charm:
                    case ItemType.Shop:
                    case ItemType.Trinket:
                        if (newItem.trinketNum > 0)
                        {
                            Actions.Add(new ChangeShinyIntoTrinket(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.trinketNum, newItem.boolName, location));
                            if (!string.IsNullOrEmpty(oldItem.altObjectName))
                            {
                                Actions.Add(new ChangeShinyIntoTrinket(oldItem.sceneName, oldItem.altObjectName,
                                    oldItem.fsmName, newItem.trinketNum, newItem.boolName, location));
                            }
                        }
                        else
                        {
                            Actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.objectName,
                                oldItem.fsmName, newItem.boolName, location));

                            if (!string.IsNullOrEmpty(oldItem.altObjectName))
                            {
                                Actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.altObjectName,
                                    oldItem.fsmName, newItem.boolName, location));
                            }
                        }
                        break;

                    // make a shiny with cutscene
                    case ItemType.Big:
                    case ItemType.Spell:
                        BigItemDef[] newItemsArray = GetBigItemDefArray(newItemName);

                        Actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.objectName,
                            oldItem.fsmName, newItemsArray, randomizerBoolName, location, playerdata));
                        if (!string.IsNullOrEmpty(oldItem.altObjectName))
                        {
                            Actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.altObjectName,
                                oldItem.fsmName, newItemsArray, randomizerBoolName, location, playerdata));
                        }

                        break;

                    // make a geo spawn
                    case ItemType.Geo:
                        if (oldItem.inChest)
                        {
                            Actions.Add(new ChangeChestGeo(oldItem.sceneName, oldItem.chestName,
                                oldItem.chestFsmName, newItem.geo, location));
                        }
                        else if (oldItem.type == ItemType.Geo)
                        {
                            Actions.Add(new ChangeChestGeo(oldItem.sceneName, oldItem.objectName, oldItem.fsmName,
                                    newItem.geo, newItem.boolName));
                        }
                        else
                        {
                            Actions.Add(new ChangeShinyIntoGeo(oldItem.sceneName, oldItem.objectName,
                                oldItem.fsmName, newItem.boolName, newItem.geo, location));

                            if (!string.IsNullOrEmpty(oldItem.altObjectName))
                            {
                                Actions.Add(new ChangeShinyIntoGeo(oldItem.sceneName, oldItem.altObjectName,
                                    oldItem.fsmName, newItem.boolName, newItem.geo, location));
                            }
                        }

                        break;
                    default:
                        throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                }

                if (oldItem.cost != 0)
                {
                    Actions.Add(new AddYNDialogueToShiny(
                        oldItem.sceneName,
                        oldItem.objectName,
                        oldItem.fsmName,
                        newItem.nameKey,
                        oldItem.cost,
                        oldItem.costType));
                }
            }

            int shopAdditiveItems = 0;
            List<ChangeShopContents> shopActions = new List<ChangeShopContents>();

            // TODO: Change to use additiveItems rather than hard coded
            // No point rewriting this before making the shop component
            foreach ((string shopItem, string shopName) in items.Where(item => shopNames.Contains(item.Item2)))
            {
                ReqDef newItem = LogicManager.GetItemDef(shopItem);

                if (newItem.type == ItemType.Spell)
                {
                    switch (newItem.boolName)
                    {
                        case "hasVengefulSpirit":
                        case "hasShadeSoul":
                            newItem.boolName = "RandomizerMod.ShopFireball" + shopAdditiveItems++;
                            break;
                        case "hasDesolateDive":
                        case "hasDescendingDark":
                            newItem.boolName = "RandomizerMod.ShopQuake" + shopAdditiveItems++;
                            break;
                        case "hasHowlingWraiths":
                        case "hasAbyssShriek":
                            newItem.boolName = "RandomizerMod.ShopScream" + shopAdditiveItems++;
                            break;
                        default:
                            throw new Exception("Unknown spell name: " + newItem.boolName);
                    }
                }
                else if (newItem.boolName == "hasDash" || newItem.boolName == "hasShadowDash")
                {
                    newItem.boolName = "RandomizerMod.ShopDash" + shopAdditiveItems++;
                }
                else if (newItem.boolName == nameof(PlayerData.hasDreamNail) ||
                         newItem.boolName == nameof(PlayerData.hasDreamGate) ||
                         newItem.boolName == nameof(PlayerData.dreamNailUpgraded))
                {
                    newItem.boolName = "RandomizerMod.ShopDreamNail" + shopAdditiveItems++;
                }
                else if (newItem.boolName.EndsWith("QueenFragment") || newItem.boolName.EndsWith("KingFragment") || newItem.boolName.EndsWith("VoidHeart"))
                {
                    newItem.boolName = "RandomizerMod.ShopKingsoul" + shopAdditiveItems++;
                }
                else if (newItem.boolName.EndsWith("Geo"))
                {
                    newItem.boolName = "RandomizerMod.ShopGeo" + newItem.geo;
                }
                else if (newItem.boolName.StartsWith("oneGeo"))
                {
                    newItem.boolName = "RandomizerMod.Shop" + newItem.boolName;
                }

                ShopItemBoolNames[(shopItem, shopName)] = newItem.boolName;

                ShopItemDef newItemDef = new ShopItemDef
                {
                    PlayerDataBoolName = newItem.boolName,
                    NameConvo = newItem.nameKey,
                    DescConvo = newItem.shopDescKey,
                    RequiredPlayerDataBool = LogicManager.GetShopDef(shopName).requiredPlayerDataBool,
                    RemovalPlayerDataBool = string.Empty,
                    DungDiscount = LogicManager.GetShopDef(shopName).dungDiscount,
                    NotchCostBool = newItem.notchCost,
                    Cost = newItem.shopCost,
                    SpriteName = newItem.shopSpriteKey
                };

                if (newItemDef.Cost == 0)// @@DEPRECATE: Items placed in shops before "this" update will either be 0 or default cost...
                {
                    newItemDef.Cost = Randomizer.RandomizeShopCost(shopItem, true);
                }

                ChangeShopContents existingShopAction = shopActions.FirstOrDefault(action =>
                    action.SceneName == LogicManager.GetShopDef(shopName).sceneName &&
                    action.ObjectName == LogicManager.GetShopDef(shopName).objectName);

                if (existingShopAction == null)
                {
                    shopActions.Add(new ChangeShopContents(LogicManager.GetShopDef(shopName).sceneName,
                        LogicManager.GetShopDef(shopName).objectName, new[] { newItemDef }));
                }
                else
                {
                    existingShopAction.AddItemDefs(new[] { newItemDef });
                }
            }

            shopActions.ForEach(action => Actions.Add(action));
        }

        private static string GetAdditivePrefix(string boolName)
        {
            return LogicManager.AdditiveItemNames.FirstOrDefault(itemSet =>
                LogicManager.GetAdditiveItems(itemSet).Contains(boolName));
        }

        private static BigItemDef[] GetBigItemDefArray(string boolName)
        {
            string prefix = GetAdditivePrefix(boolName);
            if (prefix != null)
            {
                return LogicManager.GetAdditiveItems(prefix)
                    .Select(LogicManager.GetItemDef)
                    .Select(item => new BigItemDef
                    {
                        BoolName = item.boolName,
                        SpriteKey = item.bigSpriteKey,
                        TakeKey = item.takeKey,
                        NameKey = item.nameKey,
                        ButtonKey = item.buttonKey,
                        DescOneKey = item.descOneKey,
                        DescTwoKey = item.descTwoKey
                    }).ToArray();
            }

            ReqDef item2 = LogicManager.GetItemDef(boolName);
            return new[]
            {
                new BigItemDef
                {
                    BoolName = item2.boolName,
                    SpriteKey = item2.bigSpriteKey,
                    TakeKey = item2.takeKey,
                    NameKey = item2.nameKey,
                    ButtonKey = item2.buttonKey,
                    DescOneKey = item2.descOneKey,
                    DescTwoKey = item2.descTwoKey
                }
            };
        }

        private static string GetAdditiveBoolName(string boolName, ref Dictionary<string, int> additiveCounts)
        {
            if (additiveCounts == null)
            {
                additiveCounts = LogicManager.AdditiveItemNames.ToDictionary(str => str, str => 0);
            }

            string prefix = GetAdditivePrefix(boolName);
            if (string.IsNullOrEmpty(prefix))
            {
                return null;
            }

            additiveCounts[prefix] = additiveCounts[prefix] + 1;
            AdditiveBoolNames[boolName] = prefix + additiveCounts[prefix];
            return prefix + additiveCounts[prefix];
        }

        public static void Hook()
        {
            UnHook();

            On.PlayMakerFSM.OnEnable += ProcessFSM;
        }

        public static void UnHook()
        {
            On.PlayMakerFSM.OnEnable -= ProcessFSM;
        }

        public static void ProcessFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM fsm)
        {
            orig(fsm);

            string scene = fsm.gameObject.scene.name;
            foreach (RandomizerAction action in Actions)
            {
                if (action.Type != ActionType.PlayMakerFSM)
                {
                    continue;
                }

                try
                {
                    action.Process(scene, fsm);
                }
                catch (Exception e)
                {
                    LogError(
                        $"Error processing action of type {action.GetType()}:\n{JsonUtility.ToJson(action)}\n{e}");
                }
            }
        }

        public static void EditShinies()
        {
            string scene = Ref.GM.GetSceneNameString();

            foreach (RandomizerAction action in Actions)
            {
                if (action.Type != ActionType.GameObject)
                {
                    continue;
                }

                try
                {
                    action.Process(scene, null);
                }
                catch (Exception e)
                {
                    LogError(
                        $"Error processing action of type {action.GetType()}:\n{JsonUtility.ToJson(action)}\n{e}");
                }
            }
        }

        public abstract void Process(string scene, Object changeObj);
    }
}
