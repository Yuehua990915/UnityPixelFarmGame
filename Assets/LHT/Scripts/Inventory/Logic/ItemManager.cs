using System.Collections.Generic;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Inventory
{
    [RequireComponent(typeof(DataGUID))]
    public class ItemManager : MonoBehaviour,ISaveable
    {
        public Item itemPrefab;
        //扔出物品的预制体
        public Item bounceItemPrefab;
        public Transform itemParent;

        //<sceneName，itemListInScene>
        private Dictionary<string, List<SceneItem>> sceneItemDic =
            new Dictionary<string, List<SceneItem>>();
        
        private Dictionary<string, List<SceneFurniture>> sceneFurnitureDic =
            new Dictionary<string, List<SceneFurniture>>();
        
        private Transform playTransform => FindObjectOfType<PlayerMove>().transform;

        public InventoryBag_SO toolDataBox, itemDataBox;
        
        private void Start()
        {
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            
        }
        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            EventHandler.DropItemInScene += OnDropItemInScene;
            EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventHandler.DropItemInScene -= OnDropItemInScene;
            EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            sceneItemDic.Clear();
            sceneFurnitureDic.Clear();
        }

        private void OnBuildFurnitureEvent(int ID, Vector3 mouseWorldPos)
        {
            BluePrintDetails bluePrint = InventoryManager.Instance.bluePrintDataListSo.GetBluePrintDetails(ID);
            var buildItem = Instantiate(bluePrint.buildPrefab, mouseWorldPos, Quaternion.identity, itemParent);
            if (buildItem.GetComponent<Box>())
            {
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDataAmount;
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
            }
        }

        private void OnBeforeSceneUnLoadEvent()
        {
            GetAllSceneItems();
            GetAllSceneFurniture();
        }

        private void OnAfterSceneLoadEvent()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            RecreateAllItems();
            ReBuildFurniture();
        }

        /// <summary>
        /// 玩家丢出物品时，在世界地图指定位置生成
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            //在指定位置生成物体，设置统一的父物体
            var item = Instantiate(bounceItemPrefab,pos, Quaternion.identity, itemParent);
            //Item类中，有Init方法
            //可以通过赋值ID初始化
            item.itemID = ID;
            //设置砍伐树木后，掉落物有一个向下掉落的效果
            item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up * 0.6f);
        }

        private void OnDropItemInScene(int ID, Vector3 mousePos, ItemType itemType)
        {
            if(itemType == ItemType.Seed) return;
            var item = Instantiate(bounceItemPrefab, playTransform.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            var dir = (mousePos - playTransform.position).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mousePos,dir);
        }
        
        /// <summary>
        /// 获取当前激活场景的所有Item，卸载场景前(事件)调用
        /// </summary>
        private void GetAllSceneItems()
        {
            //临时变量
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            //遍历场景中所有item，获得ID
            foreach (var item in FindObjectsOfType<Item>())
            {
                //创建SceneItem对象，填入数据
                SceneItem sceneItem = new SceneItem
                {
                    itemID = item.itemID,
                    pos = new SerializableVector3(item.transform.position)
                };
                currentSceneItems.Add(sceneItem);
            }
            //更新字典
            if (sceneItemDic.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneItemDic[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            else
            {
                sceneItemDic.Add(SceneManager.GetActiveScene().name, currentSceneItems);
            }
        }

        /// <summary>
        /// 获得场景所有家具
        /// </summary>
        private void GetAllSceneFurniture()
        {
            //临时变量
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            //遍历场景中所有item，获得ID
            foreach (var item in FindObjectsOfType<Furniture>())
            {
                //创建SceneItem对象，填入数据
                SceneFurniture sceneFurniture = new SceneFurniture
                {
                    itemID = item.itemID,
                    pos = new SerializableVector3(item.transform.position)
                };
                if (item.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }
                currentSceneFurniture.Add(sceneFurniture);
            }
            //更新字典
            if(sceneFurnitureDic.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneFurnitureDic[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            else
            {
                sceneFurnitureDic.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }
        
        /// <summary>
        /// 删除场景中所有Item，重新在地图上生成所有物品，新场景加载后(事件)调用
        /// </summary>
        private void RecreateAllItems()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            //从Dic中拿到List，TryGetValue防止报空
            if (sceneItemDic.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //不用判断，直接清场
                    foreach (var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }

                    //从Dic中获取数据，在地图上生成
                    foreach (var item in currentSceneItems)
                    {
                        Item newItem = Instantiate(itemPrefab, item.pos.ToVector3(), Quaternion.identity, itemParent);
                        newItem.itemID = item.itemID;
                        newItem.Init(item.itemID);
                    }
                }
            }
        }
        
        /// <summary>
        /// 重建场景家具
        /// </summary>
        private void ReBuildFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            //从Dic中拿到List，TryGetValue防止报空
            if (sceneFurnitureDic.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurniture))
            {
                if (currentSceneFurniture != null)
                {
                    //从Dic中获取数据，在地图上生成
                    foreach (SceneFurniture furniture in currentSceneFurniture)
                    {
                        BluePrintDetails bluePrint = InventoryManager.Instance.bluePrintDataListSo.GetBluePrintDetails(furniture.itemID);
                        var buildItem = Instantiate(bluePrint.buildPrefab, furniture.pos.ToVector3(), Quaternion.identity, itemParent);
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(furniture.boxIndex);
                        }
                    }
                }
            }
        }

        public string GUID => GetComponent<DataGUID>().guid;
        
        public GameSaveData GenerateSaveData()
        {
            //填充字典
            GetAllSceneItems();
            GetAllSceneFurniture();
            GameSaveData gameSaveData = new GameSaveData();

            gameSaveData.sceneItemDic = sceneItemDic;
            gameSaveData.sceneFurnitureDic = sceneFurnitureDic;
            
            return gameSaveData;
        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            sceneItemDic = gameSaveData.sceneItemDic;
            sceneFurnitureDic = gameSaveData.sceneFurnitureDic;
            
            //重新创建
            RecreateAllItems();
            ReBuildFurniture();
        }
    }
}

