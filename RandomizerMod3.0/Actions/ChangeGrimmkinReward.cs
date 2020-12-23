using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using UnityEngine;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.Actions
{
    public class ChangeGrimmkinReward : RandomizerAction
    {
        private readonly string _fsmName;
        private readonly string _objectName;
        private readonly string _sceneName;
        private readonly string _nameKey;
        private readonly string _spriteName;
        private readonly GiveAction _action;
        private readonly string _item;
        private readonly string _location;

        public ChangeGrimmkinReward(string sceneName, string objectName, string fsmName, string nameKey, string spriteName, GiveAction action, string item, string location)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _nameKey = nameKey;
            _spriteName = spriteName;
            // GiveItem doesn't support spawning geo, and also there's no shiny to spawn it from anyway.
            if (action == GiveAction.SpawnGeo)
            {
                action = GiveAction.AddGeo;
            }
            _action = action;
            _item = item;
            _location = location;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (!(scene == _sceneName && changeObj is PlayMakerFSM fsm && fsm.FsmName == _fsmName && fsm.gameObject.name == _objectName))
            {
                return;
            }

            FsmState get = fsm.GetState("Get");
            get.RemoveActionsOfType<IncrementPlayerDataInt>();
            get.RemoveActionsOfType<SendMessage>();
            get.AddAction(new RandomizerExecuteLambda(() => GiveItem(_action, _item, _location)));

            // Make sure the correct icon and text appear
            get.GetActionsOfType<GetLanguageString>().First().convName = _nameKey;
            get.GetActionsOfType<SetSpriteRendererSprite>().First().sprite = RandomizerMod.GetSprite(_spriteName);
        }
    }
}