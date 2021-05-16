using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using SereCore;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Settings;

namespace RandomizerMod
{
    static class CustomSkills
    {
        public static void Hook()
        {
            UnHook();
            ModHooks.Instance.GetPlayerBoolHook += SkillBoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += SkillBoolSetOverride;
            On.PlayMakerFSM.OnEnable += ShowSkillsInInventory;
            On.HeroController.CanFocus += DisableFocus;
            On.HeroController.CanDash += DisableDash;
            On.HeroController.CanAttack += DisableAttack;
        }

        public static void UnHook()
        {
            ModHooks.Instance.GetPlayerBoolHook -= SkillBoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook -= SkillBoolSetOverride;
            On.PlayMakerFSM.OnEnable -= ShowSkillsInInventory;
            On.HeroController.CanFocus -= DisableFocus;
            On.HeroController.CanDash -= DisableDash;
            On.HeroController.CanAttack -= DisableAttack;
        }


        private static bool SkillBoolGetOverride(string boolName)
        {
            switch (boolName)
            {
                // Split Dash Overrides
                case nameof(PlayerData.canDash):
                    return (Ref.SKILLS.canDashLeft ^ Ref.SKILLS.canDashRight) || Ref.PD.GetBoolInternal(nameof(PlayerData.canDash));
                case nameof(CustomSkillSaveData.hasDashAny):
                    return (Ref.SKILLS.canDashLeft ^ Ref.SKILLS.canDashRight) || Ref.PD.GetBoolInternal(nameof(PlayerData.hasDash));

                // Split Claw Overrides
                case nameof(CustomSkillSaveData.hasWalljumpLeft):
                    return Ref.SKILLS.hasWalljumpLeft;
                case nameof(CustomSkillSaveData.hasWalljumpRight):
                    return Ref.SKILLS.hasWalljumpRight;
                case nameof(PlayerData.hasWalljump):
                    if (Ref.HC.touchingWallL && Ref.SKILLS.hasWalljumpLeft && !Ref.SKILLS.hasWalljumpRight)
                    {
                        return true;
                    }
                    else if (Ref.HC.touchingWallR && Ref.SKILLS.hasWalljumpRight && !Ref.SKILLS.hasWalljumpLeft)
                    {
                        return true;
                    }
                    break;
                case nameof(CustomSkillSaveData.hasWalljumpAny):
                    return (Ref.SKILLS.hasWalljumpLeft ^ Ref.SKILLS.hasWalljumpRight) || Ref.PD.GetBoolInternal(nameof(PlayerData.hasWalljump));
            }
            return Ref.PD.GetBoolInternal(boolName);
            /*
            // bools for left and right cloak
            // canDash: Override here so they always have dash with exactly one direction, and disable it separately in the 
            // DisableDash function. If they have neither or both of the directions, we shouldn't do anything here to provide
            // minimal disruption for other mods
            if (boolName == "canDash")
            {
                return RandomizerMod.Instance.Settings.GetBool(name: "canDashLeft") && !RandomizerMod.Instance.Settings.GetBool(name: "canDashRight")
                    || RandomizerMod.Instance.Settings.GetBool(name: "canDashRight") && !RandomizerMod.Instance.Settings.GetBool(name: "canDashLeft")
                    || PlayerData.instance.GetBoolInternal("canDash");
            }
            // hasDashAny: dummy bool to check if we should be showing dash in the inventory
            if (boolName == "hasDashAny")
            {
                return RandomizerMod.Instance.Settings.GetBool(name: "canDashLeft") && !RandomizerMod.Instance.Settings.GetBool(name: "canDashRight")
                   || RandomizerMod.Instance.Settings.GetBool(name: "canDashRight") && !RandomizerMod.Instance.Settings.GetBool(name: "canDashLeft")
                   || PlayerData.instance.GetBoolInternal("hasDash");
            }

            // bools for left and right claw
            if (boolName == "hasWalljumpLeft" || boolName == "hasWalljumpRight")
            {
                return RandomizerMod.Instance.Settings.GetBool(name: boolName);
            }
            // We don't need to check if split claw is active, because this code should only execute if the player has exactly one
            // claw piece
            if (boolName == "hasWalljump")
            {
                // If the player has both claw pieces, they are considered to have claw so we don't need to do anything here. 
                // This way, if they have both claw pieces then we won't override the behaviour in case e.g. they disable claw with debug mod.
                if (RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpLeft")
                    && !RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpRight")
                    && HeroController.instance.touchingWallL)
                {
                    return true;
                }
                else if (RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpRight")
                    && !RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpLeft")
                    && HeroController.instance.touchingWallR)
                {
                    return true;
                }
            }
            // dummy bool to check if we should be showing the mantis claw in inventory
            if (boolName == "hasWalljumpAny")
            {
                return RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpLeft")
                    || RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpRight")
                    || PlayerData.instance.GetBoolInternal("hasWalljump");
            }

            return SereCore.Ref.PD.GetBoolInternal(boolName);
            */
        }

        private static void SkillBoolSetOverride(string boolName, bool value)
        {
            switch (boolName)
            {
                // bools for left and right cloak
                case nameof(CustomSkillSaveData.canDashLeft):
                    // Give the player shadowdash if they already have that dash direction
                    if (Ref.SKILLS.canDashLeft && value)
                    {
                        Ref.PD.SetBool(nameof(PlayerData.hasShadowDash), true);
                    }
                    // Otherwise, let the player dash in that direction
                    else
                    {
                        Ref.SKILLS.canDashLeft = value;
                    }
                    if (Ref.SKILLS.canDashLeft && Ref.SKILLS.canDashRight)
                    {
                        Ref.PD.SetBool(nameof(PlayerData.hasDash), true);
                    }
                    break;
                case nameof(CustomSkillSaveData.canDashRight):
                    if (Ref.SKILLS.canDashRight && value)
                    {
                        Ref.PD.SetBool(nameof(PlayerData.hasShadowDash), true);
                    }
                    else
                    {
                        Ref.SKILLS.canDashRight = value;
                    }
                    if (Ref.SKILLS.canDashLeft && Ref.SKILLS.canDashRight)
                    {
                        Ref.PD.SetBool(nameof(PlayerData.hasDash), true);
                    }
                    break;
                // bools for left and right claw
                // If the player has one piece and gets the other, then we give them the full mantis claw. This allows the split claw to work with other mods more easily, 
                // unless of course they have only one piece.
                case nameof(CustomSkillSaveData.hasWalljumpLeft):
                    Ref.SKILLS.hasWalljumpLeft = value;
                    if (value && Ref.SKILLS.hasWalljumpRight)
                    {
                        Ref.PD.SetBool(nameof(PlayerData.hasWalljump), true);
                    }
                    break;
                case nameof(CustomSkillSaveData.hasWalljumpRight):
                    Ref.SKILLS.hasWalljumpRight = value;
                    if (value && Ref.SKILLS.hasWalljumpLeft)
                    {
                        Ref.PD.SetBool(nameof(PlayerData.hasWalljump), true);
                    }
                    break;
            }
            // Send the set through to the actual set
            Ref.PD.SetBoolInternal(boolName, value);

            /*
            if (boolName == "canDashLeft" || boolName == "canDashRight")
            {
                // Give the player shadowdash if they already have that dash direction
                if (RandomizerMod.Instance.Settings.GetBool(name: boolName) && value)
                {
                    SereCore.Ref.PD.SetBool("hasShadowDash", true);
                }
                // Otherwise, let the player dash in that direction
                else
                {
                    RandomizerMod.Instance.Settings.SetBool(value, boolName);
                }
                if (RandomizerMod.Instance.Settings.GetBool(name: "canDashLeft") && RandomizerMod.Instance.Settings.GetBool(name: "canDashRight"))
                {
                    SereCore.Ref.PD.SetBool("hasDash", true);
                }
            }

            // bools for left and right claw
            // If the player has one piece and gets the other, then we give them the full mantis claw. This allows the split claw to work with other mods more easily, 
            // unless of course they have only one piece.
            else if (boolName == "hasWalljumpLeft")
            {
                RandomizerMod.Instance.Settings.SetBool(value, boolName);
                if (value && RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpRight"))
                {
                    SereCore.Ref.PD.SetBool("hasWalljump", true);
                }
            }
            else if (boolName == "hasWalljumpRight")
            {
                RandomizerMod.Instance.Settings.SetBool(value, boolName);
                if (value && RandomizerMod.Instance.Settings.GetBool(name: "hasWalljumpLeft"))
                {
                    SereCore.Ref.PD.SetBool("hasWalljump", true);
                }
            }
            */
        }

        private static void ShowSkillsInInventory(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.FsmName == "Build Equipment List" && self.gameObject.name == "Equipment")
            {
                self.GetState("Walljump").GetActionOfType<PlayerDataBoolTest>().boolName.Value = nameof(CustomSkillSaveData.hasWalljumpAny);

                PlayerDataBoolTest[] dashChecks = self.GetState("Dash").GetActionsOfType<PlayerDataBoolTest>();
                dashChecks[0].boolName.Value = nameof(CustomSkillSaveData.hasDashAny);
            }
        }

        private static bool DisableFocus(On.HeroController.orig_CanFocus orig, HeroController self)
        {
            if (RandomizerMod.Instance.Settings.RandomizeFocus && !Ref.SKILLS.canFocus) return false;
            else return orig(self);
        }

        private static bool DisableDash(On.HeroController.orig_CanDash orig, HeroController self)
        {
            // Only disable dash in a direction if they have it in the other direction. If they have both or neither dash
            // direction, then it will be handled by the original function.
            // We don't need to check if Split Cloak is active, because we only change the output if the player has exactly one cloak piece
            switch (GetDashDirection(self))
            {
                default:
                    return orig(self);
                case Direction.leftward:
                    return orig(self) && (!Ref.SKILLS.canDashRight || Ref.SKILLS.canDashLeft);
                case Direction.rightward:
                    return orig(self) && (!Ref.SKILLS.canDashLeft || Ref.SKILLS.canDashRight);
                case Direction.downward:
                    return orig(self);
            }
        }
        private static Direction GetDashDirection(HeroController hc)
        {
            InputHandler input = ReflectionHelper.GetAttr<HeroController, InputHandler>(hc, "inputHandler");
            if (!hc.cState.onGround && input.inputActions.down.IsPressed && hc.playerData.GetBool("equippedCharm_31")
                    && !(input.inputActions.left.IsPressed || input.inputActions.right.IsPressed))
            {
                return Direction.downward;
            }
            if (hc.wallSlidingL) return Direction.rightward;
            else if (hc.wallSlidingR) return Direction.leftward;
            else if (input.inputActions.right.IsPressed) return Direction.rightward;
            else if (input.inputActions.left.IsPressed) return Direction.leftward;
            else if (hc.cState.facingRight) return Direction.rightward;
            else return Direction.leftward;
        }


        private static bool DisableAttack(On.HeroController.orig_CanAttack orig, HeroController self)
        {
            switch (GetAttackDirection(self))
            {
                default:
                    return orig(self);

                case Direction.upward:
                    return orig(self) && (Ref.SKILLS.canUpslash || !RandomizerMod.Instance.Settings.CursedNail);
                case Direction.leftward:
                    return orig(self) && (Ref.SKILLS.canSideslashLeft || !RandomizerMod.Instance.Settings.CursedNail);
                case Direction.rightward:
                    return orig(self) && (Ref.SKILLS.canSideslashRight || !RandomizerMod.Instance.Settings.CursedNail);
                case Direction.downward:
                    return orig(self);
            }
        }
        // This function copies the code in HeroController.DoAttack to determine the attack direction, with an
        // additional check if the player is wallsliding (because we want to treat a wallslash as a normal slash)
        private static Direction GetAttackDirection(HeroController hc)
        {
            if (hc.wallSlidingL)
            {
                return Direction.rightward;
            }
            else if (hc.wallSlidingR)
            {
                return Direction.leftward;
            }

            if (hc.vertical_input > Mathf.Epsilon)
            {
                return Direction.upward;
            }
            else if (hc.vertical_input < -Mathf.Epsilon)
            {
                if (hc.hero_state != GlobalEnums.ActorStates.idle && hc.hero_state != GlobalEnums.ActorStates.running)
                {
                    return Direction.downward;
                }
                else
                {
                    return hc.cState.facingRight ? Direction.rightward : Direction.leftward;
                }
            }
            else
            {
                return hc.cState.facingRight ? Direction.rightward : Direction.leftward;
            }
        }

        private enum Direction
        {
            upward,
            leftward,
            rightward,
            downward
        }
    }
}
