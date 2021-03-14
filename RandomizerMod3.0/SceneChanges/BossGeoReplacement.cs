using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Components;
using RandomizerMod.FsmStateActions;
using SereCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using static RandomizerMod.LogHelper;
using System.Collections;
using RandomizerMod.SceneChanges;

namespace RandomizerMod.SceneChanges
{
    class BossGeoReplacement
    {
        public static bool GeoBossEnabled (GameObject enemy, bool isDead)
        {

            if (!RandomizerMod.Instance.Settings.RandomizeBossGeo) return isDead;

            switch (enemy.name)
            {
                default:
                    break;

                case "Mega Zombie Beam Miner (1)" when GameManager.instance.sceneName == SceneNames.Mines_18:
                    ReplaceGeoFromBoss(enemy, "Corpse Mega Zombie Beam Miner Esc", SceneNames.Mines_18);
                    break;
                case "Zombie Beam Miner Rematch" when GameManager.instance.sceneName == SceneNames.Mines_32:
                    ReplaceGeoFromBoss(enemy, "Corpse Mega Zombie Beam Miner", SceneNames.Mines_32);
                    break;

                // For some reason the soul warriors need a special check
                case "Mage Knight" when GameManager.instance.sceneName == SceneNames.Ruins1_23:
                    if (CheckIfSceneDataActivated(SceneNames.Ruins1_23, "Battle Scene v2")) break;
                    ReplaceGeoFromBoss(enemy, "Corpse Mage Knight", SceneNames.Ruins1_23); 
                    break;
                case "Mage Knight" when GameManager.instance.sceneName == SceneNames.Ruins1_31 + "b":
                    if (CheckIfSceneDataActivated(SceneNames.Ruins1_31, "Battle Scene v2")) break;
                    ReplaceGeoFromBoss(enemy, "Corpse Mage Knight", SceneNames.Ruins1_31 + "b");
                    break;

                case "Mega Moss Charger" when GameManager.instance.sceneName == SceneNames.Fungus1_29:
                    ReplaceGeoFromBoss(enemy, "Corpse Mega Moss", SceneNames.Fungus1_29);
                    break;
                case "Gorgeous Husk" when GameManager.instance.sceneName == SceneNames.Ruins_House_02:
                    ReplaceGeoFromBoss(enemy, "Corpse Flukeman Bot", SceneNames.Ruins_House_02);
                    break;

                // The Gruz Mother shiny drop replaces the geo so it's easiest to spawn it via the FSM.
                // We manually move it oob here, and bring it back in the FSM edit.
                case "Giant Fly" when GameManager.instance.sceneName == SceneNames.Crossroads_04:
                    if (GameObject.Find("New Shiny Boss Geo") is GameObject bossGeoShiny) bossGeoShiny.transform.SetPositionY(400f);
                    break;
                case "Giant Buzzer" when GameManager.instance.sceneName == SceneNames.Fungus1_20_v02:
                    // If they've picked up the item from VK with the spore shroom glitch, then we won't 
                    // run the code so they can get a duped geo spawn
                    if (RandomizerMod.Instance.Settings.CheckLocationFound("Boss_Geo-Vengefly_King")) break;
                    ReplaceGeoFromBoss(enemy, "Corpse Giant Buzzer", SceneNames.Fungus1_20_v02);
                    break;
            }

            return isDead;
        }

        // Gruz Mother needs a special case because her geo is special. Ideally we'd make the shiny spawn 
        public static void DestroyGruzmomGeo(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (!RandomizerMod.Instance.Settings.RandomizeBossGeo) return;

            if (self.gameObject.name.StartsWith("Corpse Big Fly Burster") && self.FsmName == "burster" 
                && GameManager.instance.sceneName == SceneNames.Crossroads_04)
            {
                FsmState geoState = self.GetState("Initiate");
                geoState.RemoveActionsOfType<FlingObjectsFromGlobalPool>();
                geoState.AddAction(
                    new RandomizerExecuteLambda(() => GameObject.Find("New Shiny Boss Geo").transform.SetPosition2D(
                        self.gameObject.transform.position
                        )));

                FsmState initState = self.GetState("Initiate");
                initState.ClearTransitions();
                initState.AddTransition("FINISHED", "In Air");
            }
        }

        private static void ReplaceGeoFromBoss(GameObject enemy, string corpseName, string sceneName)
        {
            HealthManager hm = enemy.GetComponent<HealthManager>();
            // Remove vanilla boss geo
            if (hm == null) return;
            hm.SetGeoSmall(0);
            hm.SetGeoMedium(0);
            hm.SetGeoLarge(0);

            // Move shiny out of bounds; return when enemy is dead
            if (GameObject.Find("New Shiny Boss Geo") is GameObject bossGeoShiny)
            {
                bossGeoShiny.transform.SetPositionY(400F);
                IEnumerator bossDead()
                {
                    yield return new WaitUntil(() => hm.GetIsDead() || GameManager.instance.sceneName != sceneName);
                    if (GameManager.instance.sceneName == sceneName)
                    {
                        GameObject bossCorpse = Object.FindObjectsOfType<GameObject>().First(obj => obj.name.Contains(corpseName));
                        bossGeoShiny.transform.SetPosition2D(bossCorpse.transform.position);
                    }
                }
                GameManager.instance.StartCoroutine(bossDead());
            }
        }

        private static bool CheckIfSceneDataActivated(string sceneName, string id)
        {
            PersistentBoolData preBoolData = new PersistentBoolData { sceneName = sceneName, id = id };
            PersistentBoolData boolData = SceneData.instance.FindMyState(preBoolData) ?? preBoolData;
            return boolData.activated && !boolData.semiPersistent;
        }
    }
}
