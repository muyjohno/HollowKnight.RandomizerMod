using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Components;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using static RandomizerMod.LogHelper;
using System.Collections;
using RandomizerMod.SceneChanges;

namespace RandomizerMod.SceneChanges
{
    internal static class RandomizerChanges
    {
        /*
         * Changes that prevent clawless softlocks, mostly
         */

        public static void FixSoftlocks(Scene newScene)
        {
            switch (newScene.name)
            {
                // Platforms to climb out of basin with only wings. Item rando exclusive
                case SceneNames.Abyss_01 when !RandomizerMod.Instance.Settings.RandomizeTransitions:
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(11f, 15f);
                        platform.SetActive(true);
                    }
                    break;

                // Platforms to climb out from basin wanderer's journal
                case SceneNames.Abyss_02:
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(128.3f, 7f);
                        platform.SetActive(true);
                    }
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(128.3f, 11f);
                        platform.SetActive(true);
                    }
                    break;

                // Platforms to climb up to tram in basin from left with no items
                case SceneNames.Abyss_03 when !RandomizerMod.Instance.Settings.RandomizeTransitions:
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(34f, 7f);
                        platform.SetActive(true);
                    }
                    break;

                // Platforms to climb out of basin with only wings. Technically not needed, except in case of max nail. Included in all versions
                case SceneNames.Abyss_04:
                    {
                        GameObject platform1 = ObjectCache.SmallPlatform;
                        platform1.transform.SetPosition2D(27f, 16f);
                        platform1.SetActive(true);

                        GameObject platform2 = ObjectCache.SmallPlatform;
                        platform2.transform.SetPosition2D(27f, 26f);
                        platform2.SetActive(true);

                        GameObject platform3 = ObjectCache.SmallPlatform;
                        platform3.transform.SetPosition2D(29.5f, 62f);
                        platform3.SetActive(true);
                    }
                    break;

                // Platform to climb out of Abyss with only wings
                case SceneNames.Abyss_06_Core:
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(88.6f, 263f);
                        platform.SetActive(true);
                    }
                    break;

                // Platforms to climb back up from pale ore with no items
                case SceneNames.Abyss_17:
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(164.7f, 30f);
                        platform.SetActive(true);
                    }
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(99.5f, 12.5f);
                        platform.SetActive(true);
                    }
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(117.7f, 18.8f);
                        platform.SetActive(true);
                    }
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(114.3f, 23f);
                        platform.SetActive(true);
                    }
                    break;
                // Platforms to remove softlock with wings at simple key in basin
                case SceneNames.Abyss_20:
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(26.5f, 13f);
                        platform.transform.SetScaleX(.8f);
                        platform.SetActive(true);
                    }
                    break;
                // Platforms to climb back up from Mantis Lords with only wings
                case SceneNames.Fungus2_15 when !RandomizerMod.Instance.Settings.RandomizeTransitions:
                    {
                        GameObject[] platforms = new GameObject[2];
                        for (int i = 0; i < 2; i++)
                        {
                            platforms[i] = ObjectCache.SmallPlatform;
                            platforms[i].transform.SetPosition2D(48f + 2 * i, 10f * i + 15f);
                            platforms[i].SetActive(true);
                        }
                    }
                    break;

                // Bounce shrooms to prevent softlock for Fungal Core start in open mode without claw
                case SceneNames.Fungus2_30:
                    {
                        GameObject bounceShroom = GameObject.Find("Bounce Shroom C");
                        
                        GameObject s0 = Object.Instantiate(bounceShroom);
                        s0.transform.SetPosition3D(12.5f, 26f, 0f);
                        s0.SetActive(true);

                        GameObject s1 = Object.Instantiate(bounceShroom);
                        s1.transform.SetPosition3D(12.5f, 54f, 0f);
                        s1.SetActive(true);

                        GameObject s2 = Object.Instantiate(bounceShroom);
                        s2.transform.SetPosition3D(21.7f, 133f, 0f);
                        s2.SetActive(true);
                    }
                    break;

                // Platforms to prevent softlock on lever on the way to love key. Didn't need as many as I expected
                case SceneNames.Fungus3_05:
                    for (int i = 0; i < 2; i++)
                    {
                        GameObject platform = ObjectCache.SmallPlatform;
                        platform.transform.SetPosition2D(65.7f, 11f + 4.5f * i);
                        platform.SetActive(true);
                    }
                    break;

                // Platforms for open mode to get out of flower quest room
                case SceneNames.Fungus3_49 when RandomizerMod.Instance.Settings.OpenMode:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            GameObject platform = ObjectCache.SmallPlatform;
                            platform.transform.SetPosition2D(75f + i, 6.5f);
                            platform.SetActive(true);
                        }
                    }
                    break;

                // Platform for open mode, might as well include it in other modes too
                case SceneNames.Hive_03:
                    GameObject hivePlatform = ObjectCache.SmallPlatform;
                    hivePlatform.transform.SetPosition2D(58.5f, 134f);
                    hivePlatform.SetActive(true);
                    break;

                // Move the load in colo downward to prevent bench soft lock
                case SceneNames.Room_Colosseum_02 when !RandomizerMod.Instance.Settings.RandomizeTransitions:
                    GameObject coloTransition1 = GameObject.Find("top1");
                    coloTransition1.transform.SetPositionY(coloTransition1.transform.position.y - 9f);
                    break;

                // Platforms to climb back up to King's Pass with no items
                case SceneNames.Town when !RandomizerMod.Instance.Settings.RandomizeTransitions:
                    {
                        GameObject[] platforms = new GameObject[6];
                        for (int i = 0; i < 6; i++)
                        {
                            platforms[i] = ObjectCache.SmallPlatform;
                            platforms[i].transform.SetPosition2D(20f - 2 * (i%2), 5f * i + 15f);
                            platforms[i].SetActive(true);
                        }
                    }
                    break;


                // Platforms to prevent itemless softlock when checking left waterways
                case SceneNames.Waterways_04 when !RandomizerMod.Instance.Settings.RandomizeTransitions:
                    {
                        GameObject[] platforms = new GameObject[4];
                        platforms[0] = ObjectCache.SmallPlatform;
                        platforms[0].transform.SetPosition2D(107f, 10f);
                        platforms[0].SetActive(true);

                        platforms[1] = ObjectCache.SmallPlatform;
                        platforms[1].transform.SetPosition2D(107f, 15f);
                        platforms[1].SetActive(true);

                        platforms[2] = ObjectCache.SmallPlatform;
                        platforms[2].transform.SetPosition2D(148f, 23.1f);
                        platforms[2].SetActive(true);

                        platforms[3] = ObjectCache.SmallPlatform;
                        platforms[3].transform.SetPosition2D(139f, 32f);
                        platforms[3].SetActive(true);
                    }
                    break;
            }
        }


        /*
         * Room changes required for the randomizer to function on any mode
         * For example, removing certain vanilla item locations which can't be handled by RandomizerAction
         */

        public static void ApplyRandomizerChanges(Scene newScene)
        {
            switch (newScene.name)
            {
                // Prevent Grimm encounter which gives Grimmchild
                case SceneNames.Grimm_Main_Tent:
                    PlayerData.instance.metGrimm = true;
                    break;

                // Prevent reading tablet which gives completion percentage
                case SceneNames.Room_Final_Boss_Atrium:
                    GameObject.Find("Tut_tablet_top").LocateMyFSM("Inspection").GetState("Init").ClearTransitions();
                    break;

                // Removes the prompt to donate to the 3000 geo fountain in Basin
                case SceneNames.Abyss_04:
                    Object.Destroy(GameObject.Find("Fountain Donation"));
                    break;

                // Opens lifeblood door in Abyss with any amount of blue health
                case SceneNames.Abyss_06_Core:
                    if (PlayerData.instance.healthBlue > 0 || PlayerData.instance.joniHealthBlue > 0 || GameManager.instance.entryGateName == "left1")
                    {
                        PlayerData.instance.SetBoolInternal("blueVineDoor", true);
                        PlayMakerFSM BlueDoorFSM = GameObject.Find("Blue Door").LocateMyFSM("Control");
                        BlueDoorFSM.GetState("Init").RemoveTransitionsTo("Got Charm");
                    }
                    break;

                // Removes trigger for Void Heart sequence
                case SceneNames.Abyss_15:
                    GameObject.Find("Dream Enter Abyss").LocateMyFSM("Control").GetState("Init").RemoveTransitionsTo("Idle");
                    GameObject.Find("Dream Enter Abyss").LocateMyFSM("Control").GetState("Init").AddTransition("FINISHED", "Inactive");
                    break;

                // Automatically unlock Godseeker and add an action to the Godtuner spot to remove simple key on purchase
                case SceneNames.GG_Waterways:
                    PlayerData.instance.SetBool("godseekerUnlocked", true);
                    if (GameObject.Find("Randomizer Shiny") != null)
                    {
                        PlayMakerFSM godtuner = GameObject.Find("Randomizer Shiny").LocateMyFSM("Shiny Control");
                        godtuner.GetState(godtuner.GetState("Charm?").Transitions.First(t => t.EventName == "YES").ToState).AddFirstAction(new RandomizerExecuteLambda(() => PlayerData.instance.DecrementInt("simpleKeys")));
                    }
                    break;

                // Spawns mawlek shard out of bounds and moves it inbounds when mawlek is killed
                case SceneNames.Crossroads_09:
                    if (GameObject.Find("Randomizer Shiny") is GameObject mawlekShard)
                    {
                        mawlekShard.transform.SetPositionY(100f);
                        IEnumerator mawlekDead()
                        {
                            yield return new WaitUntil(() => PlayerData.instance.killedMawlek);
                            mawlekShard.transform.SetPositionY(10f);
                            mawlekShard.transform.SetPositionX(61.5f);
                        }
                        GameManager.instance.StartCoroutine(mawlekDead());
                    }
                    break;

                // Removes Grubfather rewards corresponding to randomizer items
                case SceneNames.Crossroads_38:
                    Object.Destroy(GameObject.Find("Reward 5"));  //Mask
                    Object.Destroy(GameObject.Find("Reward 10")); //Charm
                    Object.Destroy(GameObject.Find("Reward 16")); //Rancid Egg
                    Object.Destroy(GameObject.Find("Reward 23")); //Relic
                    Object.Destroy(GameObject.Find("Reward 31")); //Pale Ore
                    Object.Destroy(GameObject.Find("Reward 38")); //Relic
                    Object.Destroy(GameObject.Find("Reward 46")); //Charm
                    break;

                // Remove gate from Ancestral Mound
                case SceneNames.Crossroads_ShamanTemple:
                    Object.Destroy(GameObject.Find("Bone Gate"));
                    break;

                // Remove Beast's Den hardsave, allow rear access from entrance, destroy Herrah
                case SceneNames.Deepnest_Spider_Town:
                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "Deepnest_Spider_Town",
                        id = "Collapser Small (12)",
                        activated = true,
                        semiPersistent = false
                    });

                    FsmState denHardSave = GameObject.Find("RestBench Spider").LocateMyFSM("Fade").GetState("Land");
                    denHardSave.RemoveActionsOfType<CallMethodProper>();
                    denHardSave.RemoveActionsOfType<SendMessage>();
                    denHardSave.RemoveActionsOfType<SetPlayerDataBool>();

                    Object.Destroy(GameObject.Find("Dreamer Hegemol"));
                    Object.Destroy(GameObject.Find("Dream Enter"));
                    Object.Destroy(GameObject.Find("Dream Impact"));
                    Object.Destroy(GameObject.Find("Shield"));
                    break;

                // Edits Dream Nail location to change scene to seer
                case SceneNames.Dream_Nailcollection:
                    FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish")
                        .AddAction(new RandomizerChangeScene("RestingGrounds_07", "right1"));
                    break;

                // Edit Hornet room to open gates after boss fight, and removes dreamer cutscene
                case SceneNames.Fungus1_04:
                    foreach (PlayMakerFSM childFSM in GameObject.Find("Cloak Corpse")
                        .GetComponentsInChildren<PlayMakerFSM>(true))
                    {
                        if (childFSM.FsmName == "Shiny Control")
                        {
                            SendEvent openGate = new SendEvent
                            {
                                eventTarget = new FsmEventTarget
                                {
                                    target = FsmEventTarget.EventTarget.BroadcastAll,
                                    excludeSelf = true
                                },
                                sendEvent = FsmEvent.FindEvent("BG OPEN"),
                                delay = 0,
                                everyFrame = false
                            };
                            childFSM.GetState("Destroy").AddFirstAction(openGate);
                            childFSM.GetState("Finish").AddFirstAction(openGate);

                            break;
                        }
                    }

                    ObjectDestroyer.Destroy("Dreamer Scene 1");
                    ObjectDestroyer.Destroy("Hornet Saver");
                    ObjectDestroyer.Destroy("Cutscene Dreamer");
                    ObjectDestroyer.Destroy("Dream Scene Activate");

                    if (!Ref.PD.hornet1Defeated)
                    {
                        Object.Destroy(FSMUtility.LocateFSM(GameObject.Find("Camera Locks Boss"), "FSM"));
                    }
                    break;

                // Make city crest gate openable infinite times and not hard save
                case SceneNames.Fungus2_21:
                    FSMUtility.LocateFSM(GameObject.Find("City Gate Control"), "Conversation Control")
                        .GetState("Activate").RemoveActionsOfType<SetPlayerDataBool>();

                    FsmState gateSlam = FSMUtility.LocateFSM(GameObject.Find("Ruins_gate_main"), "Open")
                        .GetState("Slam");
                    gateSlam.RemoveActionsOfType<SetPlayerDataBool>();
                    gateSlam.RemoveActionsOfType<CallMethodProper>();
                    gateSlam.RemoveActionsOfType<SendMessage>();
                    break;

                // Removes Leg Eater dialogue tree, preventing him from dying
                case SceneNames.Fungus2_26:
                    PlayMakerFSM legEater = FSMUtility.LocateFSM(GameObject.Find("Leg Eater"), "Conversation Control");
                    FsmState legEaterChoice = legEater.GetState("Convo Choice");
                    legEaterChoice.RemoveTransitionsTo("Convo 1");
                    legEaterChoice.RemoveTransitionsTo("Convo 2");
                    legEaterChoice.RemoveTransitionsTo("Convo 3");
                    legEaterChoice.RemoveTransitionsTo("Infected Crossroad");
                    legEaterChoice.RemoveTransitionsTo("Bought Charm");
                    legEaterChoice.RemoveTransitionsTo("Gold Convo");
                    legEaterChoice.RemoveTransitionsTo("All Gold");
                    legEaterChoice.RemoveTransitionsTo("Ready To Leave");
                    legEater.GetState("All Gold?").RemoveTransitionsTo("No Shop");
                    Ref.PD.legEaterLeft = false;
                    break;

                // Destroy Monomon and remove Quirrel encounter
                case SceneNames.Fungus3_archive_02:
                    PlayerData.instance.SetBool("summonedMonomon", true);
                    Object.Destroy(GameObject.Find("Inspect Region"));
                    Object.Destroy(GameObject.Find("Quirrel Wounded"));
                    Object.Destroy(GameObject.Find("Quirrel"));
                    Object.Destroy(GameObject.Find("Monomon"));
                    Object.Destroy(GameObject.Find("Dream Enter"));
                    Object.Destroy(GameObject.Find("Dream Impact"));
                    Object.Destroy(GameObject.Find("Shield"));
                    break;

                // Make tolls always interactable, in the rare case that lantern is not randomized but RG access through the dark room is expected
                case SceneNames.Mines_33:
                    if (RandomizerMod.Instance.Settings.DarkRooms && !RandomizerMod.Instance.Settings.RandomizeKeys)
                    {
                        GameObject[] tolls = new GameObject[] { GameObject.Find("Toll Gate Machine"), GameObject.Find("Toll Gate Machine (1)") };
                        foreach (GameObject toll in tolls)
                        {
                            Object.Destroy(FSMUtility.LocateFSM(toll, "Disable if No Lantern"));
                        }
                    }
                    break;

                // Move Seer back to make room for items, and remove essence rewards
                case SceneNames.RestingGrounds_07:
                    GameObject.Find("Dream Moth").transform.Translate(new Vector3(-5f, 0f));

                    PlayMakerFSM moth = FSMUtility.LocateFSM(GameObject.Find("Dream Moth"), "Conversation Control");

                    PlayerData.instance.dreamReward1 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 1").Value = true;  //Relic
                    PlayerData.instance.dreamReward3 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 3").Value = true;  //Pale Ore
                    PlayerData.instance.dreamReward4 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 4").Value = true;  //Charm
                    PlayerData.instance.dreamReward5 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 5").Value = true;  //Vessel Fragment
                    PlayerData.instance.dreamReward5b = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 5b").Value = true; //Skill
                    PlayerData.instance.dreamReward6 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 6").Value = true;  //Relic
                    PlayerData.instance.dreamReward7 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 7").Value = true;  //Mask Shard
                    PlayerData.instance.dreamReward8 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 8").Value = true;  //Skill
                    break;

                // Make Sly pickup send Sly back upstairs -- warps player out to prevent resulting softlock from trying to enter the shop from a missing transition 
                case SceneNames.Room_Sly_Storeroom:
                    FsmState slyFinish = FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish");
                    slyFinish.AddAction(new RandomizerSetBool("SlyCharm", true));
                    slyFinish.AddAction(new RandomizerChangeScene("Town", "door_sly"));
                    break;

                // Many changes to make the desolate dive pickup work properly
                case SceneNames.Ruins1_24:
                    // Stop spell container from destroying itself
                    PlayMakerFSM quakePickup = FSMUtility.LocateFSM(GameObject.Find("Quake Pickup"), "Pickup");
                    quakePickup.GetState("Idle").RemoveActionsOfType<IntCompare>();
                    foreach (PlayMakerFSM childFSM in quakePickup.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
                    {
                        if (childFSM.FsmName == "Shiny Control")
                        {
                            // Make spell container spawn shiny instead
                            quakePickup.GetState("Appear").GetActionsOfType<ActivateGameObject>()[1].gameObject
                                .GameObject.Value = childFSM.gameObject;

                            // Make shiny open gates on pickup/destroy
                            SendEvent openGate = new SendEvent
                            {
                                eventTarget = new FsmEventTarget
                                {
                                    target = FsmEventTarget.EventTarget.BroadcastAll,
                                    excludeSelf = true
                                },
                                sendEvent = FsmEvent.FindEvent("BG OPEN"),
                                delay = 0,
                                everyFrame = false
                            };
                            childFSM.GetState("Destroy").AddFirstAction(openGate);
                            childFSM.GetState("Finish").AddFirstAction(openGate);
                            break;
                        }
                    }

                    // Stop the weird invisible floor from appearing if dive has been obtained
                    if (Ref.PD.quakeLevel > 0)
                    {
                        Object.Destroy(GameObject.Find("Roof Collider Battle"));
                    }

                    // Change battle gate to be destroyed if Soul Master is dead instead of it the player has quake
                    FsmState checkQuake = FSMUtility.LocateFSM(GameObject.Find("Battle Gate (1)"), "Destroy if Quake").GetState("Check");
                    checkQuake.RemoveActionsOfType<FsmStateAction>();
                    checkQuake.AddAction(new RandomizerBoolTest(nameof(PlayerData.killedMageLord), null, "DESTROY", true));
                    break;

                // Prevent simple key softlocks
                case SceneNames.Ruins2_04:
                    FsmState hotSpringsKey = GameObject.Find("Inspect").LocateMyFSM("Conversation Control").GetState("Got Key?");
                    hotSpringsKey.RemoveActionsOfType<IntCompare>();
                    hotSpringsKey.AddAction(new RandomizerExecuteLambda(() =>
                    {
                        if (GameManager.instance.GetPlayerDataInt("simpleKeys") > 1 || (PlayerData.instance.openedWaterwaysManhole && GameManager.instance.GetPlayerDataInt("simpleKeys") > 0)) PlayMakerFSM.BroadcastEvent("YES");
                        else PlayMakerFSM.BroadcastEvent("NO");
                    }));
                    break;

                // Destroy Lurien
                case SceneNames.Ruins2_Watcher_Room:
                    Object.Destroy(GameObject.Find("Dreamer Lurien"));
                    Object.Destroy(GameObject.Find("Dream Enter"));
                    Object.Destroy(GameObject.Find("Dream Impact"));
                    Object.Destroy(GameObject.Find("Shield"));
                    break;

                // Open all colosseum trials
                case SceneNames.Room_Colosseum_01:
                    PlayerData.instance.colosseumBronzeOpened = true;
                    PlayerData.instance.colosseumSilverOpened = true;
                    PlayerData.instance.colosseumGoldOpened = true;
                    GameObject.Find("Silver Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").ClearTransitions();
                    GameObject.Find("Silver Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").AddTransition("FINISHED", "Box Up YN");
                    GameObject.Find("Gold Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").ClearTransitions();
                    GameObject.Find("Gold Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").AddTransition("FINISHED", "Box Up YN");
                    break;

                // Destroy Grey Mourner after the flower has been delivered
                case SceneNames.Room_Mansion:
                    if (PlayerData.instance.xunFlowerGiven)
                    {
                        PlayerData.instance.xunRewardGiven = true;
                    }
                    break;

                // Removes King's Brand cutscene trigger
                case SceneNames.Room_Wyrm:
                    Object.Destroy(GameObject.Find("Avalanche End"));
                    break;

                // Open Colosseum gates after picking up resp. items
                case SceneNames.Room_Colosseum_Bronze:
                    GameObject.Find("Colosseum Manager").LocateMyFSM("Geo Pool").GetState("Open Gates").AddFirstAction(new RandomizerSetBool("colosseumBronzeCompleted", true, true));
                    break;
                case SceneNames.Room_Colosseum_Silver:
                    GameObject.Find("Colosseum Manager").LocateMyFSM("Geo Pool").GetState("Open Gates").AddFirstAction(new RandomizerSetBool("colosseumSilverCompleted", true, true));
                    break;

                // Prevent simple key softlocks
                case SceneNames.Town:
                    FsmState jijiKey = GameObject.Find("Jiji Door").LocateMyFSM("Conversation Control").GetState("Key?");
                    jijiKey.RemoveActionsOfType<GetPlayerDataInt>();
                    jijiKey.RemoveActionsOfType<IntCompare>();
                    jijiKey.AddAction(new RandomizerExecuteLambda(() =>
                    {
                        if (GameManager.instance.GetPlayerDataInt("simpleKeys") > 1 || (PlayerData.instance.openedWaterwaysManhole && GameManager.instance.GetPlayerDataInt("simpleKeys") > 0)) PlayMakerFSM.BroadcastEvent("KEY");
                        else PlayMakerFSM.BroadcastEvent("NOKEY");
                    }));
                    break;
            }
        }
    }
}
