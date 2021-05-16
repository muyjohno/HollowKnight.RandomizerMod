﻿using SereCore;
using UnityEngine;
using Random = System.Random;

namespace RandomizerMod.Components
{
    internal class RandomizerTinkEffect : MonoBehaviour
    {
        private static readonly Random Rnd = new Random();

        private float _nextTime;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Nail Attack") || Time.time < _nextTime)
            {
                return;
            }

            _nextTime = Time.time + 0.25f;

            GameCameras cam = GameCameras.instance;
            if (cam != null)
            {
                cam.cameraShakeFSM.SendEvent("EnemyKillShake");
            }

            float degrees = 0f;
            PlayMakerFSM damagesEnemy = PlayMakerFSM.FindFsmOnGameObject(collision.gameObject, "damages_enemy");
            if (damagesEnemy != null)
            {
                degrees = damagesEnemy.FsmVariables.FindFsmFloat("direction").Value;
            }

            Vector3 position = SereCore.Ref.Hero.transform.position;
            Vector3 euler = Vector3.zero;
            switch (DirectionUtils.GetCardinalDirection(degrees))
            {
                case 0:
                    SereCore.Ref.Hero.RecoilLeft();
                    position = new Vector3(position.x + 2, position.y, 0.002f);
                    break;
                case 1:
                    SereCore.Ref.Hero.RecoilDown();
                    position = new Vector3(position.x, position.y + 2, 0.002f);
                    euler = new Vector3(0, 0, 90);
                    break;
                case 2:
                    SereCore.Ref.Hero.RecoilRight();
                    position = new Vector3(position.x - 2, position.y, 0.002f);
                    euler = new Vector3(0, 0, 180);
                    break;
                default:
                    position = new Vector3(position.x, position.y - 2, 0.002f);
                    euler = new Vector3(0, 0, 270);
                    break;
            }

            GameObject effect = ObjectCache.TinkEffect;
            effect.transform.localPosition = position;
            effect.transform.localRotation = Quaternion.Euler(euler);
            effect.GetComponent<AudioSource>().pitch = (85 + Rnd.Next(30)) / 100f;

            effect.SetActive(true);
        }
    }
}