using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SereCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using RandomizerMod.FsmStateActions;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.Actions
{
    public class ReplaceObjectWithGeoRock : RandomizerAction
    {
        private readonly string _objectName;
        private readonly string _sceneName;
        private readonly string _rockName;
        private readonly string _item;
        private readonly string _location;
        private readonly float _elevation;
        private readonly int _geo;

        public const float GEO_ROCK_HIVE_ELEVATION = -0.2f;
        public const float GEO_ROCK_MINE_ELEVATION = 0.1f;

        public ReplaceObjectWithGeoRock(string sceneName, string objectName, float elevation, string rockName, string item, string location, int geo)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _rockName = rockName;
            _item = item;
            _location = location;
            _elevation = elevation;
            _geo = geo;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName)
            {
                return;
            }

            Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            string[] objectHierarchy = _objectName.Split('\\');
            int i = 1;
            GameObject obj = currentScene.FindGameObject(objectHierarchy[0]);
            while (i < objectHierarchy.Length)
            {
                obj = obj.FindGameObjectInChildren(objectHierarchy[i++]);
            }

            if (obj == null) return;

            // Put a geo rock in the same location as the original
            GameObject rock = ObjectCache.GeoRock;
            rock.name = _rockName;
            if (obj.transform.parent != null)
            {
                rock.transform.SetParent(obj.transform.parent);
            }

            rock.transform.position = obj.transform.position;
            rock.transform.localPosition = obj.transform.localPosition;
            rock.transform.position += Vector3.up * (HIVE_GEO_ROCK_ELEVATION - _elevation);
            rock.SetActive(obj.activeSelf);

            SetGeo(rock);


            // Destroy the original
            Object.Destroy(obj);
        }

        private void SetGeo(GameObject rock) {
            var fsm = FSMUtility.LocateFSM(rock, "Geo Rock");
            var init = fsm.GetState("Initiate");
            init.RemoveActionsOfType<IntCompare>();
            init.AddAction(new RandomizerExecuteLambda(() => {
                fsm.SendEvent(RandomizerMod.Instance.Settings.CheckLocationFound(_location) ? "BROKEN" : null);
            }));
            var hit = fsm.GetState("Hit");
            hit.ClearTransitions();
            hit.AddTransition("HIT", "Pause Frame");
            hit.AddTransition("FINISHED", "Pause Frame");
            var payout = fsm.GetState("Destroy");
            var payoutAction = payout.GetActionOfType<FlingObjectsFromGlobalPool>();
            payoutAction.spawnMin.Value = _geo - 6;
            payoutAction.spawnMax.Value = _geo - 6;
            payout.AddAction(new RandomizerExecuteLambda(() => GiveItem(GiveAction.None, _item, _location)));
        }
    }
}