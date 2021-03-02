using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SereCore;
using UnityEngine;
using RandomizerMod.FsmStateActions;

namespace RandomizerMod.Actions
{
    internal class CreateNewGrubJar : RandomizerAction
    {
        private readonly string _newGrubJarName;
        private readonly string _sceneName;
        private readonly float _x;
        private readonly float _y;
        private readonly string _location;

        public CreateNewGrubJar(string sceneName, float x, float y, string newGrubJarName, string location)
        {
            _sceneName = sceneName;
            _x = x;
            _y = y + 1;
            _newGrubJarName = newGrubJarName;
            _location = location;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName)
            {
                return;
            }

            GameObject GrubJar = ObjectCache.GrubJar;
            GrubJar.name = _newGrubJarName;

            GrubJar.transform.position = new Vector3(_x, _y, GrubJar.transform.position.z);
            GrubJar.AddComponent<Rigidbody2D>();

            FixBottleFSM(GrubJar, _location);
            
            GrubJar.SetActive(true);
        }

        public static void FixBottleFSM(GameObject jar, string location)
        {
            var fsm = FSMUtility.LocateFSM(jar, "Bottle Control");
            var init = fsm.GetState("Init");
            init.RemoveActionsOfType<BoolTest>();
            init.AddFirstAction(new RandomizerExecuteLambda(() => fsm.SendEvent(RandomizerMod.Instance.Settings.CheckLocationFound(location) ? "ACTIVATE" : null)));
            fsm.GetState("Shatter").AddAction(new RandomizerExecuteLambda(() => RandomizerMod.Instance.Settings.MarkLocationFound(location)));
        }
    }
}
