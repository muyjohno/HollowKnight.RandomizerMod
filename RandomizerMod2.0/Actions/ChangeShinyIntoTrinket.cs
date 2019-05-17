using System;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ChangeShinyIntoTrinket : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private int trinketNum;

        public ChangeShinyIntoTrinket(string sceneName, string objectName, string fsmName, int trinketNum)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.trinketNum = trinketNum;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != sceneName || !(changeObj is PlayMakerFSM fsm) || fsm.FsmName != fsmName || fsm.gameObject.name != objectName)
            {
                return;
            }

            //trinketNum = 20;

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");
            FsmState trinkFlash = fsm.GetState("Trink Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Force the FSM to follow the path for the correct trinket
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Trink Flash");
            trinkFlash.ClearTransitions();
            fsm.GetState("Trinket Type").ClearTransitions();
            
            if (trinketNum == 1) trinkFlash.AddTransition("FINISHED", "Trink 1");
            else if (trinketNum == 2) trinkFlash.AddTransition("FINISHED", "Trink 2");
            else if (trinketNum == 3) trinkFlash.AddTransition("FINISHED", "Trink 3");
            else if (trinketNum == 4) trinkFlash.AddTransition("FINISHED", "Trink 4");
            else if (trinketNum == 6) trinkFlash.AddTransition("FINISHED", "Tram pass");
            else if (trinketNum == 8) trinkFlash.AddTransition("FINISHED", "Store Key");
            else if (trinketNum == 9) trinkFlash.AddTransition("FINISHED", "City Key");
            else if (trinketNum == 10) trinkFlash.AddTransition("FINISHED", "Love Key");
            else if (trinketNum == 11) trinkFlash.AddTransition("FINISHED", "Egg");
            else if (trinketNum == 12)
                {
                    trinkFlash.AddTransition("FINISHED", "Simple Key");
                    fsm.GetState("Simple Key").RemoveActionsOfType<SetPlayerDataBool>();
                    RandomizerMod.Instance.Log("Regular path works fine");
                }
            else if (trinketNum == 13)
                {
                    trinkFlash.AddTransition("FINISHED", "Notch");
                    fsm.GetState("Notch").RemoveActionsOfType<SetPlayerDataBool>();
                }
            else if (trinketNum == 14)
                {
                    trinkFlash.AddTransition("FINISHED", "Ore");
                    fsm.GetState("Ore").RemoveActionsOfType<SetPlayerDataBool>();
                }
            else if (trinketNum == 20)
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = "hasLantern";
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "INV_NAME_LANTERN";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.Lantern");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
            else if (trinketNum == 21)
            {
                fsm.GetState("Store Key").GetActionsOfType<SetPlayerDataBool>().First().boolName = "hasWhiteKey";
                fsm.GetState("Store Key").GetActionsOfType<GetLanguageString>().First().convName = "INV_NAME_WHITEKEY";
                fsm.GetState("Store Key").GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite("ShopIcons.ElegantKey");
                trinkFlash.AddTransition("FINISHED", "Store Key");
            }
        }
    }
}
