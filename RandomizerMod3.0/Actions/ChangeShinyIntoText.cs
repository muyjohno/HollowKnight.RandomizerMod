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
        private readonly bool _majorLore;

        public ChangeShinyIntoText(string sceneName, string objectName, string fsmName, string key, string sheetTitle, 
            bool majorLore, string item, string location)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _item = item;
            _location = location;
            _key = key;
            _sheetTitle = sheetTitle;
            _majorLore = majorLore;
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

            if (_majorLore)
            {
                startReading.AddAction(new RandomizerExecuteLambda(() => AudioSource.PlayClipAtPoint(
                    ObjectCache.LoreSound,
                    GameObject.Find(changeObj.name).transform.position
                    )));

                startReading.AddAction(new RandomizerExecuteLambda(() => PlayMakerFSM.BroadcastEvent("LORE PROMPT UP")));
            }
            else
            {
                startReading.AddAction(new RandomizerExecuteLambda(() => GameObject.Find("DialogueManager")
                    .LocateMyFSM("Box Open").SendEvent("BOX UP")));
            }
            startReading.AddAction(new Wait()
            {
                time = _majorLore ? 0.85f : 0.3f,
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
                _key, _sheetTitle, _majorLore));

            // Finished Reading
            FsmState finishReading = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Lore Finish Reading"
            };
            finishReading.ClearTransitions();
            finishReading.RemoveActionsOfType<FsmStateAction>();

            finishReading.AddAction(new RandomizerCallStaticMethod(GetType(), nameof(HideLoreDialogue), _majorLore));
            if (_majorLore) finishReading.AddAction(new Wait()
            {
                time = 0.5f,
                finishEvent = FsmEvent.Finished
            });

            // Once we're done we have to reset the text box
            fsm.GetState("Flash").AddFirstAction(new RandomizerCallStaticMethod(GetType(), nameof(ResetTextBox)));

            // Cancel Reading (Hero Damaged)
            FsmState cancelReading = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Lore Cancel Reading"
            };
            cancelReading.ClearTransitions();
            cancelReading.RemoveActionsOfType<FsmStateAction>();

            cancelReading.AddAction(new RandomizerCallStaticMethod(GetType(), nameof(HideLoreDialogue), _majorLore));
            cancelReading.AddAction(new RandomizerCallStaticMethod(GetType(), nameof(ResetTextBox)));
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



        private static void ShowLoreDialogue(GameObject shiny, string key, string sheetTitle, bool majorLore)
        {
            GameObject dialogueManager = GameObject.Find("DialogueManager");
            GameObject textObj = dialogueManager.transform.Find("Text").gameObject;
            textObj.LocateMyFSM("Dialogue Page Control").FsmVariables.GetFsmGameObject("Requester").Value = shiny;

            // Set position of text box
            if (majorLore)
            {
                textObj.transform.SetPositionY(2.44f);
                dialogueManager.transform.Find("Stop").gameObject.transform.SetPositionY(-0.23f);
                dialogueManager.transform.Find("Arrow").gameObject.transform.SetPositionY(-0.3f);
            }

            textObj.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.Top;
            textObj.GetComponent<DialogueBox>().StartConversation(key, sheetTitle);
        }

        private static void HideLoreDialogue(bool majorLore)
        {
            GameObject dialogueManager = GameObject.Find("DialogueManager");
            if (!majorLore) dialogueManager.LocateMyFSM("Box Open").SendEvent("BOX DOWN");
            if (majorLore) PlayMakerFSM.BroadcastEvent("LORE PROMPT DOWN");
        }

        private static void ResetTextBox()
        {
            GameObject dialogueManager = GameObject.Find("DialogueManager");

            GameObject textObj = dialogueManager.transform.Find("Text").gameObject;
            textObj.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.TopLeft;
            dialogueManager.transform.Find("Arrow").gameObject.transform.SetPositionY(1.695f);
            textObj.transform.SetPositionY(4.49f);
            dialogueManager.transform.Find("Stop").gameObject.transform.SetPositionY(1.695f);
        }
    }
}
