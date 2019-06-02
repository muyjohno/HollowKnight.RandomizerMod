using System;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.FsmStateActions;
using RandomizerMod.Extensions;
using SeanprCore;
using UnityEngine;

namespace RandomizerMod.Actions
{
    // ReSharper disable once UnusedMember.Global
    public class ChangeShinyIntoTrinket : RandomizerAction
    {
        private readonly string _fsmName;
        private readonly string _objectName;
        private readonly string _sceneName;
        private readonly int _trinketNum;
        private readonly string _boolName;

        public ChangeShinyIntoTrinket(string sceneName, string objectName, string fsmName, int trinketNum, string boolName)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _trinketNum = trinketNum;
            _boolName = boolName;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName || !(changeObj is PlayMakerFSM fsm) || fsm.FsmName != _fsmName ||
                fsm.gameObject.name != _objectName)
            {
                return;
            }


            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");
            FsmState trinkFlash = fsm.GetState("Trink Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Add our own check to stop the shiny from being grabbed twice
            pdBool.AddAction(new RandomizerBoolTest(boolName, null, "COLLECTED"));

            // Force the FSM to follow the path for the correct trinket
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Trink Flash");
            trinkFlash.ClearTransitions();
            fsm.GetState("Trinket Type").ClearTransitions();

            if (trinketNum == 1) trinkFlash.AddTransition("FINISHED", "Trink 1");
            else if (trinketNum == 2) trinkFlash.AddTransition("FINISHED", "Trink 2");
            else if (trinketNum == 3) trinkFlash.AddTransition("FINISHED", "Trink 3");
            else if (trinketNum == 4)
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = boolName;
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "RANDOMIZER_NAME_ARCANE_EGG";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.ArcaneEgg");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 6) trinkFlash.AddTransition("FINISHED", "Tram pass");
            else if (trinketNum == 8) trinkFlash.AddTransition("FINISHED", "Store Key");
            else if (trinketNum == 9) trinkFlash.AddTransition("FINISHED", "City Key");
            else if (trinketNum == 10) trinkFlash.AddTransition("FINISHED", "Love Key");
            else if (trinketNum == 11) trinkFlash.AddTransition("FINISHED", "Egg");
            else if (trinketNum == 12)
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = boolName;
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "INV_NAME_SIMPLEKEY";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.SimpleKey");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 13)
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = boolName;
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "RANDOMIZER_NAME_CHARM_NOTCH";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.CharmNotch");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 14)
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = boolName;
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "RANDOMIZER_NAME_PALE_ORE";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.PaleOre");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 20) //lantern
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = "hasLantern";
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "INV_NAME_LANTERN";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.Lantern");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 21) //elegant key
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = "hasWhiteKey";
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "INV_NAME_WHITEKEY";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.ElegantKey");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 22) //mask shards
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = boolName;
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "RANDOMIZER_NAME_MASK_SHARD";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.MaskShard");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 23) //vessel fragments
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = boolName;
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "RANDOMIZER_NAME_VESSEL_FRAGMENT";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.VesselFragment");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
        }
    }
}