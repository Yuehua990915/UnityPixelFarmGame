using System;
using Farm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TMP_InputField tradeAmount;
    public Button submitButton;
    public Button cancelButton;

    private ItemDetails item;
    private bool isSellTrade;


    private void Awake()
    {
        cancelButton.onClick.AddListener(CancelTrade);
        submitButton.onClick.AddListener(TradeItem);
    }

    
    public void SetupTradeUI(ItemDetails item, bool isSell)
    {
        this.item = item;
        itemIcon.sprite = item.icon;
        itemName.text = item.itemName;
        isSellTrade = isSell;
        tradeAmount.text = string.Empty;
    }

    private void TradeItem()
    {
        int amount;
        if (int.TryParse(tradeAmount.text, out amount))
        {
            InventoryManager.Instance.TradeItem(item, amount, isSellTrade);
        }
    
        CancelTrade();
    }

    private void CancelTrade()
    {
        gameObject.SetActive(false);
    }
}