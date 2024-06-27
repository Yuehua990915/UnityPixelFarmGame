using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.NPC
{
    public class NPCManager : Singleton<NPCManager>
    {
        //获得所有场景中的NPC，新类型，List
        //为NPC赋予初始场景、坐标
        public List<NPCPosition> npcPositionList;

        public SceneRouteDataList_SO routeData;
        private Dictionary<string, SceneRoute> routeDic = new Dictionary<string, SceneRoute>();

        protected override void Awake()
        {
            base.Awake();
            InitRouteDic();
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            //初始化每个npc的位置、场景
            foreach (var character in npcPositionList)
            {
                character.npc.position = character.position;
                character.npc.GetComponent<NPCMovement>().currentScene = character.startScene;
            }
        }

        private void InitRouteDic()
        {
            if (routeData.sceneRouteList.Count > 0)
            {
                foreach (var route in routeData.sceneRouteList)
                {
                    string key = route.fromSceneName + route.gotoSceneName;
                    if (routeDic.ContainsKey(key))
                        continue;
                    else
                        routeDic.Add(key, route);
                }
            }
        }

        public SceneRoute GetRouteFromDic(string fromSceneName, string gotoSceneName)
        {
            return routeDic[fromSceneName + gotoSceneName];
        }
    }
}