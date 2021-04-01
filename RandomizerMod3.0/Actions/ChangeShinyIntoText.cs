using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.FsmStateActions;
using SereCore;
using UnityEngine;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Actions
{
    public class ChangeShinyIntoText : RandomizerAction
    {
        private readonly string _item;
        private readonly string _fsmName;
        private readonly string _objectName;
        private readonly string _sceneName;
        private readonly string _location;
        private readonly string _key;
        private readonly string _sheetTitle;

        public ChangeShinyIntoText(string sceneName, string objectName, string fsmName, string key, string sheetTitle, string item, string location)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _item = item;
            _location = location;
            _key = key;
            _sheetTitle = sheetTitle;
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
            FsmState getCharm = fsm.GetState("Get Charm");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Add our own check to stop the shiny from being grabbed twice
            pdBool.AddAction(
                new RandomizerExecuteLambda(() => fsm.SendEvent(
                    RandomizerMod.Instance.Settings.CheckLocationFound(_location) ? "COLLECTED" : null
                    )));

            // The "Charm?" state is a bad entry point for our lore showing
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Get Charm");
            // The "Get Charm" state is a good entry point for our lore showing
            getCharm.RemoveActionsOfType<SetPlayerDataBool>();
            getCharm.RemoveActionsOfType<IncrementPlayerDataInt>();
            getCharm.RemoveActionsOfType<SendMessage>();

            getCharm.AddAction(new RandomizerExecuteLambda(() => GiveItemActions.GiveItem(GiveItemActions.GiveAction.None, _item, _location)));
            getCharm.ClearTransitions();

            // Begin showing lore state
            FsmState startReading = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Lore Start Reading"
            };
            startReading.ClearTransitions();
            startReading.RemoveActionsOfType<FsmStateAction>();

            startReading.AddAction(new RandomizerExecuteLambda(() => GameObject.Find("DialogueManager")
                .LocateMyFSM("Box Open").SendEvent("BOX UP")));
            startReading.AddAction(new Wait()
            {
                time = 0.3f,
                finishEvent = FsmEvent.Finished
            });

            // Reading
            FsmState loreReading = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Lore Reading"
            };
            loreReading.ClearTransitions();
            loreReading.RemoveActionsOfType<FsmStateAction>();

            loreReading.AddAction(new RandomizerCallStaticMethod(GetType(), nameof(ShowLoreDialogue), fsm.gameObject,
                _key, _sheetTitle));

            // Finished Reading
            FsmState finishReading = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Lore Finish Reading"
            };
            finishReading.ClearTransitions();
            finishReading.RemoveActionsOfType<FsmStateAction>();

            finishReading.AddAction(new RandomizerExecuteLambda(() => GameObject.Find("DialogueManager")
                .LocateMyFSM("Box Open").SendEvent("BOX DOWN")));

            // Cancel Reading (Hero Damaged)
            FsmState cancelReading = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Lore Cancel Reading"
            };
            cancelReading.ClearTransitions();
            cancelReading.RemoveActionsOfType<FsmStateAction>();

            cancelReading.AddAction(new RandomizerExecuteLambda(() => GameObject.Find("DialogueManager")
                .LocateMyFSM("Box Open").SendEvent("BOX DOWN")));
            // Spaghetti because for some reason, the code in "Finish" doesn't yeet the inspect region, leading to softlocks
            cancelReading.AddAction(new RandomizerExecuteLambda(() => Object.Destroy(GameObject.Find(changeObj.name))));

            // Adding states
            getCharm.AddTransition("FINISHED", "Lore Start Reading");
            startReading.AddTransition("FINISHED", "Lore Reading");
            startReading.AddTransition("HERO DAMAGED", "Lore Cancel Reading");
            loreReading.AddTransition("CONVO_FINISH", "Lore Finish Reading");
            loreReading.AddTransition("HERO DAMAGED", "Lore Cancel Reading");
            finishReading.AddTransition("FINISHED", "Flash");
            cancelReading.AddTransition("FINISHED", "Finished");

            fsm.AddState(startReading);
            fsm.AddState(loreReading);
            fsm.AddState(finishReading);
            fsm.AddState(cancelReading);

        }



        private static void ShowLoreDialogue(GameObject shiny, string key, string sheetTitle)
        {
            GameObject dialogueManager = GameObject.Find("DialogueManager");

            GameObject textObj = dialogueManager.transform.Find("Text").gameObject;
            textObj.LocateMyFSM("Dialogue Page Control").FsmVariables.GetFsmGameObject("Requester").Value = shiny;
            textObj.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.Top;
            textObj.GetComponent<DialogueBox>().StartConversation(key, sheetTitle);
        }
    }
}
