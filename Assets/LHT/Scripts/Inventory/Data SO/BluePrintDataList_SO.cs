using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BluePrintDataList_SO", menuName = "Inventory/BluePrintDataList_SO")]
public class BluePrintDataList_SO : ScriptableObject
{
    public List<BluePrintDetails> bluePrintDetailsList;

    public BluePrintDetails GetBluePrintDetails(int itemID)
    {
        return bluePrintDetailsList.Find(b => b.ID == itemID);
    }
}

[System.Serializable]
public class BluePrintDetails
{
    public int ID;
    public InventoryItem[] resourceItem = new InventoryItem[4];
    public GameObject buildPrefab;
}