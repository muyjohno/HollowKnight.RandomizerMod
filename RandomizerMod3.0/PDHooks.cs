using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using RandomizerMod.Extensions;

namespace RandomizerMod
{
    public static class PDHooks
    {
        public static void Hook()
        {
            ModHooks.Instance.GetPlayerIntHook += IntOverride;
            ModHooks.Instance.GetPlayerBoolHook += BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += BoolSetOverride;
            On.PlayerData.CountGameCompletion += RandomizerCompletion;
            On.PlayerData.SetInt += FixGrimmkinUpgradeCost;
        }

        public static void UnHook()
        {
            ModHooks.Instance.GetPlayerIntHook -= IntOverride;
            ModHooks.Instance.GetPlayerBoolHook -= BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook -= BoolSetOverride;
            On.PlayerData.CountGameCompletion -= RandomizerCompletion;
            On.PlayerData.SetInt -= FixGrimmkinUpgradeCost;
        }

        // Some items require two bools to function normally
        static Dictionary<string, string> _secondaryBools = new Dictionary<string, string>
        {
            {nameof(PlayerData.hasDash), nameof(PlayerData.canDash)},
            {nameof(PlayerData.hasShadowDash), nameof(PlayerData.canShadowDash)},
            { nameof(PlayerData.hasSuperDash), nameof(PlayerData.canSuperDash)},
            { nameof(PlayerData.hasWalljump), nameof(PlayerData.canWallJump)},
            { nameof(PlayerData.gotCharm_23), nameof(PlayerData.fragileHealth_unbreakable)},
            { nameof(PlayerData.gotCharm_24), nameof(PlayerData.fragileGreed_unbreakable)},
            { nameof(PlayerData.gotCharm_25), nameof(PlayerData.fragileStrength_unbreakable)}
        };

        private static void RandomizerCompletion(On.PlayerData.orig_CountGameCompletion orig, PlayerData self)
        {
            if (!Ref.SET.Randomizer)
            {
                orig(self);
                return;
            }

            float placedItems = (float)RandomizerMod.Instance.Settings.GetNumLocations();
            float foundItems = (float)RandomizerMod.Instance.Settings.GetItemsFound().Length;

            // Count a pair (in, out) as a single transition check
            float randomizedTransitions = RandomizerMod.Instance.Settings.RandomizeRooms ? 445f :
                                            RandomizerMod.Instance.Settings.RandomizeAreas ? 80f : 0f;
            float foundTransitions = (float)RandomizerMod.Instance.Settings.GetTransitionsFound().Length / 2f;
            if (placedItems == 0 && randomizedTransitions == 0)
            {
                PlayerData.instance.completionPercentage = 0;
                return;
            }

            float rawPercent = (foundItems + foundTransitions) / (placedItems + randomizedTransitions) * 100f;

            PlayerData.instance.completionPercentage = (float)Math.Floor(rawPercent);
        }

        private static void UpdateCharmNotches(PlayerData pd)
        {
            // Update charm notches
            if (Ref.SET.GameSettings.SalubraNotches)
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
                SereCore.Ref.GM.RefreshOvercharm();
            }
        }

        private static bool BoolGetOverride(string boolName)
        {
            // TODO: delete the fake spell bools and use the ItemChanger system instead
            // Fake spell bools
            if (boolName == "hasVengefulSpirit")
            {
                return SereCore.Ref.PD.fireballLevel > 0;
            }

            if (boolName == "hasShadeSoul")
            {
                return SereCore.Ref.PD.fireballLevel > 1;
            }

            if (boolName == "hasDesolateDive")
            {
                return SereCore.Ref.PD.quakeLevel > 0;
            }

            if (boolName == "hasDescendingDark")
            {
                return SereCore.Ref.PD.quakeLevel > 1;
            }

            if (boolName == "hasHowlingWraiths")
            {
                return SereCore.Ref.PD.screamLevel > 0;
            }

            if (boolName == "hasAbyssShriek")
            {
                return SereCore.Ref.PD.screamLevel > 1;
            }

            // This variable is incredibly stubborn, not worth the effort to make it cooperate
            // Just override it completely
            if (boolName == nameof(PlayerData.gotSlyCharm) && Ref.SET.Randomizer)
            {
                return Ref.EVENTS.SlyCharm;
            }

            if (boolName == nameof(PlayerData.spiderCapture))
            {
                return false;
            }

            // Make Happy Couple require obtaining whatever item Sheo gives, instead of Great Slash
            if (boolName == nameof(PlayerData.nailsmithSheo) && Ref.POOL.Skills)
            {
                return Ref.GME.NPCItemDialogue
                    && PlayerData.instance.GetBoolInternal(nameof(PlayerData.nailsmithSpared)) 
                    && Ref.PLACEMENTS.CheckLocationFound("Great_Slash");
            }

            if (boolName == nameof(PlayerData.corniferAtHome))
            {
                if (!Ref.POOL.Maps)
                {
                    return PlayerData.instance.GetBoolInternal(boolName);
                }
                return !Ref.GME.NPCItemDialogue || (
                       Ref.PLACEMENTS.CheckLocationFound("Greenpath_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Fog_Canyon_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Fungal_Wastes_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Deepnest_Map-Upper") &&
                       Ref.PLACEMENTS.CheckLocationFound("Deepnest_Map-Right_[Gives_Quill]") &&
                       Ref.PLACEMENTS.CheckLocationFound("Ancient_Basin_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Kingdom's_Edge_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("City_of_Tears_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Royal_Waterways_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Howling_Cliffs_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Crystal_Peak_Map") &&
                       Ref.PLACEMENTS.CheckLocationFound("Queen's_Gardens_Map"));
            }

            if (boolName == nameof(PlayerData.instance.openedMapperShop))
            {
                // Iselda is now always unlocked
                return true || PlayerData.instance.GetBoolInternal(boolName) ||
                    (!RandomizerMod.Instance.Settings.RandomizeMaps &&
                    (
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_cityLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_abyssLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_cliffsLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_crossroadsLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_deepnestLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_fogCanyonLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_fungalWastesLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_greenpathLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_minesLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_outskirtsLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_royalGardensLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_waterwaysLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.openedRestingGrounds))
                    ));
            }

            if (boolName.StartsWith("RandomizerMod."))
            {
                // format is RandomizerMod.GiveAction.ItemName.LocationName for shop bools. Only the item name is used for savesettings bools
                return Ref.PLACEMENTS.CheckItemFound(boolName.Split('.')[2]);
            }

            if (RandomizerMod.Instance.Settings.RandomizeRooms && (boolName == "troupeInTown" || boolName == "divineInTown")) return false;
            //if (boolName == "crossroadsInfected" && RandomizerMod.Instance.Settings.RandomizeRooms
            //    && new List<string> { SceneNames.Crossroads_03, SceneNames.Crossroads_06, SceneNames.Crossroads_10, SceneNames.Crossroads_19 }.Contains(GameManager.instance.sceneName)) return false;

            return SereCore.Ref.PD.GetBoolInternal(boolName);
        }

        private static void BoolSetOverride(string boolName, bool value)
        {
            // TODO: delete the fake spell bools and use the ItemChanger system instead
            PlayerData pd = SereCore.Ref.PD;

            // It's just way easier if I can treat spells as bools
            if (boolName == "hasVengefulSpirit" && value && pd.fireballLevel <= 0)
            {
                pd.SetInt("fireballLevel", 1);
            }
            else if (boolName == "hasVengefulSpirit" && !value)
            {
                pd.SetInt("fireballLevel", 0);
            }
            else if (boolName == "hasShadeSoul" && value)
            {
                pd.SetInt("fireballLevel", 2);
            }
            else if (boolName == "hasShadeSoul" && !value && pd.fireballLevel >= 2)
            {
                pd.SetInt("fireballLevel", 1);
            }
            else if (boolName == "hasDesolateDive" && value && pd.quakeLevel <= 0)
            {
                pd.SetInt("quakeLevel", 1);
            }
            else if (boolName == "hasDesolateDive" && !value)
            {
                pd.SetInt("quakeLevel", 0);
            }
            else if (boolName == "hasDescendingDark" && value)
            {
                pd.SetInt("quakeLevel", 2);
            }
            else if (boolName == "hasDescendingDark" && !value && pd.quakeLevel >= 2)
            {
                pd.SetInt("quakeLevel", 1);
            }
            else if (boolName == "hasHowlingWraiths" && value && pd.screamLevel <= 0)
            {
                pd.SetInt("screamLevel", 1);
            }
            else if (boolName == "hasHowlingWraiths" && !value)
            {
                pd.SetInt("screamLevel", 0);
            }
            else if (boolName == "hasAbyssShriek" && value)
            {
                pd.SetInt("screamLevel", 2);
            }
            else if (boolName == "hasAbyssShriek" && !value && pd.screamLevel >= 2)
            {
                pd.SetInt("screamLevel", 1);
            }

            else if (boolName.StartsWith("RandomizerMod."))
            {
                // format is RandomizerMod.GiveAction.ItemName.LocationName for shop bools. Only the item name is used for savesettings bools

                string[] pieces = boolName.Split('.');
                pieces[1].TryToEnum(out GiveAction giveAction);
                string item = pieces[2];
                string location = pieces[3];

                GiveItemActions.GiveItem(giveAction, item, location);
                return;
            }
            // Send the set through to the actual set
            pd.SetBoolInternal(boolName, value);

            // Check if there is a secondary bool for this item
            if (_secondaryBools.TryGetValue(boolName, out string secondaryBoolName))
            {
                pd.SetBool(secondaryBoolName, value);
            }

            if (boolName == nameof(PlayerData.hasCyclone) || boolName == nameof(PlayerData.hasUpwardSlash) ||
                boolName == nameof(PlayerData.hasDashSlash))
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
                FSMUtility.LocateFSM(SereCore.Ref.Hero.gameObject, "Dream Nail").FsmVariables
                    .GetFsmBool("Dream Warp Allowed").Value = true;
            }
            else if (boolName == nameof(PlayerData.hasAcidArmour) && value)
            {
                // Gotta update the acid pools after getting this
                PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR");
            }
            else if (boolName == nameof(PlayerData.hasShadowDash) && value)
            {
                // Apparently this is enough to disable the shade gate walls
                EventRegister.SendEvent("GOT SHADOW DASH");
            }
            else if (boolName.StartsWith("gotCharm_"))
            {
                // Check for Salubra notches if it's a charm
                UpdateCharmNotches(pd);
            }
        }

        private static int IntOverride(string intName)
        {
            if (intName == "RandomizerMod.Zero")
            {
                return 0;
            }
            // Grimm only appears in his tent if the player has exactly 3 flames. Hide any excess
            // flames (which can only happen when flames are randomized) from the game.
            // Increments of the variable (collecting flames) will still increment the real value.
            if (Ref.POOL.GrimmkinFlames && intName == "flamesCollected")
            {
                int n = SereCore.Ref.PD.GetIntInternal(intName);
                return n > 3 ? 3 : n;
            }

            return SereCore.Ref.PD.GetIntInternal(intName);
        }

        // When upgrading Grimmchild, Grimm sets the flame counter to 0. If there are excess flames,
        // this is wrong; we want those flames to carry over to the next level.
        // To avoid conflicts with other mods, we hook PlayerData.SetInt directly rather than
        // use SetPlayerIntHook; when using the latter, other mods using that hook, such as
        // PlayerDataTracker, will inadvertently overwrite our changes if their hook runs after ours,
        // since they only see the value the game originally tried to set and SetPlayerIntHook
        // requires the hook to write the new value itself even if it doesn't want to override it.
        private static void FixGrimmkinUpgradeCost(On.PlayerData.orig_SetInt orig, PlayerData pd, string intName, int newValue)
        {
            if (Ref.POOL.GrimmkinFlames && intName == "flamesCollected" && newValue == 0)
            {
                // We can still get the original value here, since we haven't called orig yet.
                newValue = pd.GetIntInternal(intName) - 3;
            }
            orig(pd, intName, newValue);
        }

    }
}
