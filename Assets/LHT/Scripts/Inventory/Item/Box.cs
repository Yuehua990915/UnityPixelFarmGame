using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBag_SO boxBagTemplate;
        //根据模版生成该箱子的SO
        public InventoryBag_SO boxBagData;

        public GameObject mouseIcon;
        private bool canOpen = false;
        private bool isOpen;
        //切换场景时会归零，在ItemManager中保存序号
        public int index;
        
        private void OnEnable()
        {
            if (boxBagData == null)
            {
                boxBagData = Instantiate(boxBagTemplate);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isOpen && canOpen && Input.GetMouseButtonDown(0))
            {
                //打开箱子
                EventHandler.CallBaseBagOpenEvent(SlotType.Box,boxBagData);
                isOpen = true;
            }

            if (!canOpen && isOpen)
            {
                //关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box,boxBagData);
                isOpen = false;
            }
            
            if (isOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
                {
                    //关闭箱子
                    EventHandler.CallBaseBagCloseEvent(SlotType.Box,boxBagData);
                    isOpen = false;
                }
            }
        }

        /// <summary>
        /// 传入id，得到箱子数据
        /// </summary>
        /// <param name="boxIndex"></param>
        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            var key = name + index;
            if (InventoryManager.Instance.GetBoxDataList(key) != null)
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            //新建箱子
            else
                InventoryManager.Instance.AddBoxDataDic(this);
        }
    }
}