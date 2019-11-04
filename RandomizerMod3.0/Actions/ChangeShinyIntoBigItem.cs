using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Components;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using UnityEngine;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.Actions
{
    public struct BigItemDef
    {
        public string Name;
        public string BoolName;
        public string SpriteKey;
        public string TakeKey;
        public string NameKey;
        public string ButtonKey;
        public string DescOneKey;
        public string DescTwoKey;
    }


    public class ChangeShinyIntoBigItem : RandomizerAction
    {
        private readonly string _sceneName;
        private readonly string _objectName;
        private readonly string _fsmName;

        private readonly BigItemDef[] _itemDefs;

        private readonly GiveAction _action;        
        private readonly string _item;
        private readonly string _location;

        // BigItemDef array is meant to be for additive items
        // For example, items[0] could be vengeful spirit and items[1] would be shade soul
        public ChangeShinyIntoBigItem(string sceneName, string objectName, string fsmName, BigItemDef[] items, GiveAction action,
            string item, string location)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            
            _itemDefs = items;

            _action = action;
            _item = item;
            _location = location;
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
            FsmState bigGetFlash = fsm.GetState("Big Get Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<StringCompare>();

            // Change pd bool test to our new bool
            PlayerDataBoolTest boolTest = pdBool.GetActionsOfType<PlayerDataBoolTest>()[0];
            RandomizerBoolTest randBoolTest = new RandomizerBoolTest(_item, boolTest.isFalse, boolTest.isTrue);
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.AddFirstAction(randBoolTest);

            // Force the FSM to show the big item flash
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Big Get Flash");

            //bigGetFlash.AddAction(new RandomizerExecuteLambda(() => RandoLogger.LogItemToTrackerByBoolName(logBoolName, _location)));
            //bigGetFlash.AddAction(new RandomizerExecuteLambda(() => RandoLogger.UpdateHelperLog()));
            //bigGetFlash.AddFirstAction(new RandomizerExecuteLambda(() => RandomizerMod.Instance.Settings.UpdateObtainedProgressionByBoolName(logBoolName)));
            // Set bool and show the popup after the flash
            bigGetFlash.AddAction(new RandomizerCallStaticMethod(
                typeof(BigItemPopup),
                nameof(BigItemPopup.ShowAdditive),
                _itemDefs,
                fsm.gameObject,
                "GET ITEM MSG END"));

            // Don't actually need to set the skill here, that happens in BigItemPopup
            // Maybe change that at some point, it's not where it should happen
            bigGetFlash.AddAction(new RandomizerExecuteLambda(() => GiveItem(_action, _item, _location)));

            // Exit the fsm after the popup
            bigGetFlash.ClearTransitions();
            bigGetFlash.AddTransition("GET ITEM MSG END", "Hero Up");
            bigGetFlash.AddTransition("HERO DAMAGED", "Finish");
        }
    }
}
