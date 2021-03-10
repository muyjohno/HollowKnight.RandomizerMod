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

                case "Mega Zombie Beam Miner (1)" when GameManager.instance.sceneName == SceneNames.Mines_18: //  .StartsWith(SceneNames.Mines_18):
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
            }

            return isDead;
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
                    yield return new WaitUntil(() => hm.GetIsDead() || GameManager.instance.sceneName != sceneName); //  (GameManager.instance.sceneName != sceneName && GameManager.instance.sceneName.StartsWith(sceneName + "_boss")));
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
