﻿using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.FsmStateActions;
using SereCore;
using UnityEngine;

namespace RandomizerMod.Actions
{
    public class ChangeChestGeo : RandomizerAction
    {
        private readonly string _fsmName;
        private readonly int _geoAmount;
        private readonly string _objectName;

        private readonly string _sceneName;
        private readonly string _item;
        private readonly string _location;

        public ChangeChestGeo(string sceneName, string objectName, string fsmName, int geoAmount, string item, string location)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _geoAmount = geoAmount;
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

            // Remove actions that activate shiny item
            FsmState spawnItems = fsm.GetState("Spawn Items");
            spawnItems.RemoveActionsOfType<ActivateAllChildren>();
            fsm.GetState("Activated").RemoveActionsOfType<ActivateAllChildren>();

            // Add geo to chest
            // Chest geo pool cannot be trusted, often spawns less than it should
            spawnItems.AddAction(new RandomizerAddGeo(fsm.gameObject, _geoAmount));
            spawnItems.AddAction(new RandomizerExecuteLambda(() => GiveItemActions.GiveItem(GiveAction.None, _item, _location)));

            // Remove pre-existing geo from chest
            foreach (FlingObjectsFromGlobalPool fling in spawnItems.GetActionsOfType<FlingObjectsFromGlobalPool>())
            {
                fling.spawnMin = 0;
                fling.spawnMax = 0;
            }

            // Need to check SpawnFromPool action too because of Mantis Lords chest
            foreach (SpawnFromPool spawn in spawnItems.GetActionsOfType<SpawnFromPool>())
            {
                spawn.spawnMin = 0;
                spawn.spawnMax = 0;
            }
        }
    }
}
