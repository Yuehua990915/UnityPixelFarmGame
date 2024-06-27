using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        private SlotUI _slotUI;
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        private void Awake()
        {
            _slotUI = GetComponent<SlotUI>();
        }

        /// <summary>
        /// 启动Tooltip UI
        /// 需要拿到InventoryUI中的ItemTooltip
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_slotUI.itemDetails != null)
            {
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetTooltip(_slotUI.itemDetails,_slotUI.slotType);
                //调整tooltip出现的位置
                inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 10;
                
                //为家具蓝图时，显示所需资源UI
                if (_slotUI.itemDetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.itemTooltip.resourcePanel.gameObject.SetActive(true);
                    //拿到数据，调用SetUp方法
                    inventoryUI.itemTooltip.SetUpResourcePanel(_slotUI.itemDetails.itemID);
                }
                else
                {
                    inventoryUI.itemTooltip.resourcePanel.gameObject.SetActive(false);
                }
            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }
    }
}

