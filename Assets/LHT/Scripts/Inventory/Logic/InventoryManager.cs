using System.Collections.Generic;
using Farm.CropPlant;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Inventory
{
    [RequireComponent(typeof(DataGUID))]
    public class InventoryManager : Singleton<InventoryManager>, ISaveable
    {
        //在脚本面板绑定数据库
        [Header("物品数据")]
        public ItemDataList_SO _itemDataListSo;
        [Header("蓝图数据")]
        public BluePrintDataList_SO bluePrintDataListSo;

        [Header("玩家背包数据")] 
        public InventoryBag_SO playerBagTemp;
        public InventoryBag_SO playerBag;
        private int playerMoney;

        private InventoryBag_SO currentBoxBag;

        private Dictionary<string, List<InventoryItem>> boxDataDic = new Dictionary<string, List<InventoryItem>>();
        public int BoxDataAmount => boxDataDic.Count;
        
        private void OnEnable()
        {
            EventHandler.DropItemInScene += OnDropItemInScene;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.DropItemInScene -= OnDropItemInScene;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            playerBag = Instantiate(playerBagTemp);
            playerMoney = Settings.newGamePlayerMoney;
            //清空箱子自动
            boxDataDic.Clear();
            //更新玩家背包UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_So)
        {
            //在箱子打开时，拿到当前箱子的数据
            currentBoxBag = bag_So;
        }

        private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
        {
            //不移除蓝图
            BluePrintDetails bluePrint = bluePrintDataListSo.GetBluePrintDetails(ID);
            foreach (var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }

        private void OnHarvestAtPlayerPosition(int ID)
        {
            var index = GetItemIndexInBag(ID);
            AddItemAtIndex(ID, index, 1);
            //刷新物品栏UI，更新收获数据
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }

        private void OnDropItemInScene(int ID, Vector3 pos , ItemType itemType)
        {
            if (itemType == ItemType.Seed && CropManager.Instance.isSeedAvailableInSeason == false)
            {
                return;
            }
            RemoveItem(ID,1);
        }

        //在游戏开始时执行，读取包内数据
        private void Start()
        {
            // playerMoney = playerBag.money;
            // EventHandler.CallPlayerMoneyChangedEvent(playerMoney);
            // EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        /// <summary>
        /// 通过ID查找对应的数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int id)
        {
            return _itemDataListSo.itemDetailList.Find(i => i.itemID == id);
        }

        /// <summary>
        /// 拾取物品到背包，同时判断是否需要在画面中删除对象
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory">是否要销毁物品</param>
        public void AddItem(Item item, int amount, bool toDestory)
        {
            //在向背包中添加物品时不能使用List.Add(会增加背包上限)
            //判断是否有空位
            //是否已经有该物品，有则直接添加数量即可
            var index = GetItemIndexInBag(item.itemID);
            AddItemAtIndex(item.itemID, index, amount);
            Debug.Log(item.itemID + "Name" + GetItemDetails(item.itemID).itemName);
            if (toDestory)
            {
                Destroy(item.gameObject);
            }
            
            //更新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 检查背包是否有空位
        /// </summary>
        /// <returns></returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 通过ID索引物品在那个编号的格子中
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// 在索引位置添加物品
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            //背包中没有该物品 且 背包中有空位
            if (index == -1 && CheckBagCapacity())
            {
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    //循环到第一个空位，添加
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            //背包中已经有该物品
            else
            {
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                //更新数量
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                playerBag.itemList[index] = item;
            }
        }

        /// <summary>
        /// 背包范围内数据交换
        /// </summary>
        /// <param name="fromIndex">起始序号</param>
        /// <param name="targetIndex">目标位置序号</param>
        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];
            
            //判断目标物品位置是否为空
            if (targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[targetIndex] = currentItem;
            }
            else
            {
                playerBag.itemList[targetIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }
            //更新背包数据
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }

        /// <summary>
        /// Box、玩家背包交换数据
        /// </summary>
        /// <param name="locationFrom"></param>
        /// <param name="fromIndex"></param>
        /// <param name="locationTarget"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(InventoryLocation locationFrom, int fromIndex, 
                             InventoryLocation locationTarget, int targetIndex)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);

            InventoryItem currentItem = currentList[fromIndex];

            if (targetIndex < targetList.Count)
            {
                InventoryItem targetItem = targetList[targetIndex];

                //有物品且不相同时才进行交换（无物品/相同时是添加）
                if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                {
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;
                }
                //两物品相同时
                else if(currentItem.itemID == targetItem.itemID)
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    //为空
                    currentList[fromIndex] = new InventoryItem();
                }
                //目标格子为空
                else
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                //同时更新背包和Box的UI
                EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
                EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
            }
        }
        
        /// <summary>
        /// 返回 Box / PlayerBag 的数据列表
        /// 由于不知道是 Box -> PlayerBag 还是Player -> BagBox，
        /// 所以需要该方法来判断并返回数据
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null,
            };
        }

        /// <summary>
        /// 移除指定数量的背包物品
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="removeAmount"></param>
        public void RemoveItem(int ID, int removeAmount)
        {
            int index = GetItemIndexInBag(ID);

            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                //移除后剩余数量
                int amount = playerBag.itemList[index].itemAmount - removeAmount;
                //创建一个新的item，添加到背包
                InventoryItem item = new InventoryItem { itemID = ID, itemAmount = amount };
                playerBag.itemList[index] = item;
            }
            else if (playerBag.itemList[index].itemAmount == removeAmount)
            {
                InventoryItem item = new InventoryItem();
                playerBag.itemList[index] = item;
            }
            //更新
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }

        /// <summary>
        /// 拿到当前物品在背包中的个数
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int GetItemAmount(int ID)
        {
            int index = GetItemIndexInBag(ID);
            if (index == -1)
            {
                return 0;
            }
            return playerBag.itemList[index].itemAmount;
        }

        /// <summary>
        /// 交易物品
        /// </summary>
        /// <param name="itemDetails"></param>
        /// <param name="amount"></param>
        /// <param name="isSell">是否卖东西</param>
        public void TradeItem(ItemDetails itemDetails, int amount, bool isSell)
        {
            int cost = itemDetails.itemPrice * amount;
            int index = GetItemIndexInBag(itemDetails.itemID);
            //卖
            if (isSell)
            {
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID,amount);
                    cost = (int)(cost * itemDetails.salePercentage);
                    playerMoney += cost;
                }
            }                
            //买
            else if (playerMoney - cost >= 0)
            {
                //检查背包容量
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID,index,amount);
                }
                playerMoney -= cost;
            }

            playerBag.money = playerMoney;
            EventHandler.CallPlayerMoneyChangedEvent(playerMoney);
            //刷新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 检查建造物品库存
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluePrintDataListSo.GetBluePrintDetails(ID);
            foreach (var item in bluePrintDetails.resourceItem)
            {
                //玩家背包中的建造物品数量
                InventoryItem itemStock = playerBag.GetInventoryItem(item.itemID);
                //素材足够建造
                if (itemStock.itemAmount >= item.itemAmount)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 从字典中查找箱子的物品数据列表
        /// </summary>
        /// <param name="key">箱子ID+Name</param>
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (boxDataDic.ContainsKey(key))
                return boxDataDic[key];
            return null;
        }

        /// <summary>
        /// 将箱子中的物品列表存入Dic
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataDic(Box box)
        {
            var key = box.name + box.index;
            if (!boxDataDic.ContainsKey(key))
            {
                boxDataDic.Add(key,box.boxBagData.itemList);
            }
        }

        public string GUID => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData gameSaveData = new GameSaveData();
            //背包、箱子信息
            gameSaveData.inventoryDic = new Dictionary<string, List<InventoryItem>>();
            gameSaveData.inventoryDic.Add(playerBag.name, playerBag.itemList);

            //遍历每一个箱子
            foreach (var item in boxDataDic)
            {
                gameSaveData.inventoryDic.Add(item.Key, item.Value);
            }
            //金钱
            gameSaveData.playerMoney = playerMoney;

            return gameSaveData;
        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            //防止报空
            playerBag = Instantiate(playerBagTemp);
            playerBag.itemList = gameSaveData.inventoryDic[playerBag.name];
            
            foreach (var item in gameSaveData.inventoryDic)
            {
                if (boxDataDic.ContainsKey(item.Key))
                {
                    boxDataDic[item.Key] = item.Value;
                }
            }

            playerMoney = gameSaveData.playerMoney;

            //刷新UI，由于玩家有工具栏，所以需要刷新玩家背包
            //箱子无需刷新，打开时会刷新
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
            EventHandler.CallPlayerMoneyChangedEvent(playerMoney);
        }
    }
}

