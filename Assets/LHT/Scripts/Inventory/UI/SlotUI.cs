using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// 手动添加
        /// 手动添加在获取组件时快于Awake()
        /// 且Update中实时调用时不容易出错
        /// </summary>
        [Header("组件获取")] [SerializeField] private Image slotImage;

        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHighLight;
        [SerializeField] private Button button;

        [Header("格子类型")] 
        public SlotType slotType;

        public bool isSelected;

        public ItemDetails itemDetails;

        public int itemAmount;

        public int slotIndex;

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }
        
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        /// <summary>
        /// 在游戏开始时设置空格子
        /// </summary>
         private void Start()
        {
            isSelected = false;
            if (itemDetails == null)
                UpdateEmptySlot();
        }

        /// <summary>
        /// 更新格子UI信息
        /// </summary>
        /// <param name="item">ItemDetails</param>
        /// <param name="amount">持有数量</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.icon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            slotImage.enabled = true;
            button.interactable = true;
        }

        /// <summary>
        /// 设置Slot为空时的状态
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;
                //清空高亮
                inventoryUI.UpdateSlotHighLight(-1);
                EventHandler.CallSelectedEvent(itemDetails, isSelected);
            }
            itemDetails = null;
            //item图片禁用
            slotImage.enabled = false;
            //数量文本显示为空
            amountText.text = string.Empty;
            //按键不可点击
            button.interactable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemDetails == null) return;
            isSelected = !isSelected;
            inventoryUI.UpdateSlotHighLight(slotIndex);

            if (slotType == SlotType.Bag)
            {
                EventHandler.CallSelectedEvent(itemDetails,isSelected);
            }
        }

        //在点击拖拽时生成图片
        //而不是直接拖拽对象
        public void OnBeginDrag(PointerEventData eventData)
        {
            //TODO：不确定
            if (itemDetails != null)
            {
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                //防止图片尺寸过大导致失真
                inventoryUI.dragItem.SetNativeSize();

                isSelected = true;
                inventoryUI.UpdateSlotHighLight(slotIndex);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;

            //判断拖拽到的位置是否为空
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                //判断是不是物品格子
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;
                //获取待交换slot的序号
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;

                //相互交换的格子都应该是Bag类型
                //如果是box/shop则对应了 存放和买卖操作
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
                //买
                else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                {
                    EventHandler.CallShowTradeUI(itemDetails,false);
                }
                //卖
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                {
                    EventHandler.CallShowTradeUI(itemDetails,true);
                }
                //箱子和背包之间的交换
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && 
                         slotType != targetSlot.slotType)
                {
                    //跨背包数据交换物品
                    InventoryManager.Instance.SwapItem(Location, slotIndex, 
                        targetSlot.Location, targetSlot.slotIndex);
                }
                
                //拖拽结束时清空所有高亮
                inventoryUI.UpdateSlotHighLight(-1);
            }
            // else
            // {
            //     if (itemDetails.canDropped)
            //     {
            //         //鼠标坐标
            //         //屏幕坐标转世界坐标，camera默认z轴为-10
            //         var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            //             -Camera.main.transform.position.z));
            //
            //         EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
            //     }
            // }
        }
    } 
}