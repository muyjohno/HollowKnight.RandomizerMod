using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SereCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using RandomizerMod.FsmStateActions;

namespace RandomizerMod.Actions
{
    public class ReplaceObjectWithGrubJar : RandomizerAction
    {
        private readonly string _objectName;
        private readonly string _sceneName;
        private readonly string _jarName;
        private readonly string _location;

        public ReplaceObjectWithGrubJar(string sceneName, string objectName, string jarName, string location)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _jarName = jarName;
            _location = location;
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

            // Put a jar in the same location as the original
            GameObject jar = ObjectCache.GrubJar;
            jar.name = _jarName;
            if (obj.transform.parent != null)
            {
                jar.transform.SetParent(obj.transform.parent);
            }

            jar.transform.position = obj.transform.position;
            jar.transform.localPosition = obj.transform.localPosition;
            jar.AddComponent<Rigidbody2D>();
            if (_objectName.Contains("Shiny"))
            {
                jar.transform.position += Vector3.up * 1.4f;
            }
            jar.SetActive(obj.activeSelf);

            CreateNewGrubJar.FixBottleFSM(jar, _location);


            // Destroy the original
            Object.Destroy(obj);
        }
    }
}