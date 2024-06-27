using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Farm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private List<ISaveable> saveableList = new List<ISaveable>();

        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        public string jsonFolder;

        private int currentDataIndex;
        
        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
            
            ReadDataOnGameStart();
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }

        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;
        }

        /// <summary>
        /// 注册方法
        /// </summary>
        /// <param name="saveable"></param>
        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
            {
                saveableList.Add(saveable);
            }
        }

        public void Save(int index)
        {
            DataSlot data = new DataSlot();

            foreach (var saveable in saveableList)
            {
                data.dataDic.Add(saveable.GUID, saveable.GenerateSaveData());
            }

            dataSlots[index] = data;
            //生成存储路径
            var path = jsonFolder + "data" + index + ".json";
            //序列化
            var jsonData = JsonConvert.SerializeObject(dataSlots[index],Formatting.Indented);

            //创建路径
            if (!File.Exists(path))
            {
                //创建文件夹
                Directory.CreateDirectory(jsonFolder);
            }
            //生成文件
            File.WriteAllText(path, jsonData);
        }

        public void Load(int index)
        {
            currentDataIndex = index;
            var path = jsonFolder  + "data" + index + ".json";
            //读取文件
            var stringData = File.ReadAllText(path);
            //反序列化
            var data = JsonConvert.DeserializeObject<DataSlot>(stringData);

            foreach (var saveable in saveableList)
            {
                saveable.RestoreData(data.dataDic[saveable.GUID]);
            }
        }

        /// <summary>
        /// 在游戏开始时，读取所有存档
        /// </summary>
        private void ReadDataOnGameStart()
        {
            //如果有存档文件夹路径，则遍历读取存档
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < dataSlots.Count; i++)
                {
                    var path = jsonFolder + "data" + i + ".json";
                    //如果有该存档，读取
                    if (File.Exists(path))
                    {
                        string jsonData = File.ReadAllText(path);
                        //反序列化
                        DataSlot slotdata = JsonConvert.DeserializeObject<DataSlot>(jsonData);
                        //添加到List
                        dataSlots[i] = slotdata;
                    }
                }
            }
        }
    }
}