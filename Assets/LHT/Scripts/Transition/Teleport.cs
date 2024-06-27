using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Transition
{
    public class Teleport : MonoBehaviour
    {
        //传送到哪个场景
        [SceneName]
        public string sceneToGo;
        //传送到哪个位置
        [Header("Vector3")]
        public Vector3 positionToGo;

        private void OnTriggerEnter2D(Collider2D other)
        {
            //防止npc通过该回调传送
            if (other.CompareTag("Player"))
            {
                EventHandler.CallTransitionEvent(sceneToGo,positionToGo);
            }
        }
    }
}

