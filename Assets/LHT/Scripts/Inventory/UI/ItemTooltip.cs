using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Farm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text priceText;
    [SerializeField] private GameObject bottom;

    //Tooltips蓝图显示UI
    [SerializeField] public Transform resourcePanel;
    [SerializeField] private GameObject resourcePrefab;

    /// <summary>
    /// 给弹窗赋值
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="slotType"></param>
    public void SetTooltip(ItemDetails itemDetails, SlotType slotType)
    {
        nameText.text = itemDetails.itemName;
        //typeText.text = itemDetails.itemType.ToString();
        typeText.text = GetItemType(itemDetails.itemType);
        descriptionText.text = itemDetails.itemDetail;

        //当类型为商品、种子、家具、杂物时显示价格
        if (itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture ||
            itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.ReapableScenery)
        {
            bottom.SetActive(true);
            //获取价格
            var price = itemDetails.itemPrice;
            //判断物品是在商店还是在背包
            //背包中有折扣
            if (slotType == SlotType.Bag)
            {
                price = (int)(price * itemDetails.salePercentage);
            }

            priceText.text = price.ToString();
        }
        else
        {
            bottom.SetActive(false);
        }

        //当文本变成多行 或 多行变单行时，UI不会实时渲染回合适的大小
        //此时调用该方法，恢复合适大小
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void SetUpResourcePanel(int ID)
    {
        //拿到蓝图信息
        var bluePrint = InventoryManager.Instance.bluePrintDataListSo.GetBluePrintDetails(ID);
        //清空预制体
        if (resourcePanel.childCount > 0)
        {
            for (int i = 0; i < resourcePanel.childCount; i++)
            {
                Destroy(resourcePanel.GetChild(i).gameObject);
            }
        }

        //重新生成预制体
        foreach (var resource in bluePrint.resourceItem)
        {
            var prefab = Instantiate(resourcePrefab, resourcePanel);
            prefab.GetComponent<Image>().sprite = InventoryManager.Instance.GetItemDetails(resource.itemID).icon;
            prefab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = resource.itemAmount.ToString();
        }
    }

/// <summary>
    /// 中文显示物品类型
    /// 使用C# 8.0语法糖
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private string GetItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.Seed => "种子",
            ItemType.ReapableScenery => "杂物",
            ItemType.ChopTool => "斧子",
            ItemType.BreakTool => "镐子",
            ItemType.ReapTool => "镰刀",
            ItemType.HoeTool => "锄头",
            ItemType.CollectTool => "菜篮",
            ItemType.WaterTool => "水壶",
            _ => "无"
        };
    }
}
