using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{ 
    public class InventoryUI : MonoBehaviour
    {
        [Header("玩家背包UI")] 
        [SerializeField] 
        private GameObject bagUI;
        
        //背包打开状态
        private bool isBagOpened;
        
        [SerializeField]
        private SlotUI[] playerSlots;

        [Header("玩家金币")] 
        public TextMeshProUGUI playerMoney;
        
        [Header("通用背包")] 
        [SerializeField] 
        private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        
        [SerializeField]
        private List<SlotUI> baseBagSlots;

        [Header("交易UI")] public TradeUI tradeUI;
        
        [Header("拖拽图片")] 
        public Image dragItem;

        public ItemTooltip itemTooltip;
        
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
            EventHandler.PlayerMoneyChangedEvent += OnPlayerMoneyChangedEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
            EventHandler.PlayerMoneyChangedEvent -= OnPlayerMoneyChangedEvent;
        }

        private void OnPlayerMoneyChangedEvent(int money)
        {
            playerMoney.text = money.ToString();
        }

        private void OnShowTradeUI(ItemDetails itemDetails, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(itemDetails, isSell);
        }

        private void OnBeforeSceneUnLoadEvent()
        {
            //场景切换前，取消高亮显示
            UpdateSlotHighLight(-1);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagSo)
        {
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };
            
            //打开UI
            baseBag.SetActive(true);
            baseBagSlots = new List<SlotUI>();
            //遍历NPC背包数据
            for (int i = 0; i < bagSo.itemList.Count; i++)
            {
                if (baseBag.transform.GetChild(1).childCount < bagSo.itemList.Count)
                {
                    //生成格子
                    var slot = Instantiate(prefab, baseBag.transform.GetChild(1)).GetComponent<SlotUI>();
                    slot.slotIndex = i;
                    baseBagSlots.Add(slot);
                }
            }
            //强制刷新
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-0.6f,0.5f);
                bagUI.SetActive(true);
                isBagOpened = true;
            
            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagSo.itemList);
        }
        
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagSo)
        {
            baseBag.SetActive(false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHighLight(-1);

            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();
            
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0.5f);
                bagUI.SetActive(false);
                isBagOpened = false;
            }
        }
        
        private void Start()
        {
            //给格子序号
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }
            isBagOpened = bagUI.activeInHierarchy;
            //更新金币
            //playerMoney.text = InventoryManager.Instance.playerMoney.ToString();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.B))
                OpenBagUI();
            if (isBagOpened)
            {
                if(Input.GetKeyDown(KeyCode.Escape))
                    OpenBagUI();
            }
        }

        /// <summary>
        /// 更新指定位置的Slot事件函数
        /// </summary>
        /// <param name="location">库存位置</param>
        /// <param name="list">数据列表</param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item,list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }

                    playerMoney.text = InventoryManager.Instance.playerBag.money.ToString();
                    break;
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item,list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }
            //更新金币
            //playerMoney.text = InventoryManager.Instance.playerMoney.ToString();
        }

        /// <summary>
        /// 控制背包开关
        /// </summary>
        public void OpenBagUI()
        {
            isBagOpened = !isBagOpened;
            bagUI.SetActive(isBagOpened);
            PlayerMove.Instance.inputDisable = isBagOpened;
            itemTooltip.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新高亮显示
        /// </summary>
        /// <param name="index"></param>
        public void UpdateSlotHighLight(int index)
        {
            foreach (var slot in playerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighLight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHighLight.gameObject.SetActive(false);
                }
            }
        }
        
    }
}

