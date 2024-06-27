using System;
using Farm.Map;
using UnityEngine;

public class CropGenerator : MonoBehaviour
{
    private Grid currentGrid;
    public int seedID;
    public int growthDay;
    private void Awake()
    {
        currentGrid = FindObjectOfType<Grid>();
    }

    private void OnEnable()
    {
        EventHandler.GenerateCropEvent += CreateCrop;
    }

    private void OnDisable()
    {
        EventHandler.GenerateCropEvent -= CreateCrop;
    }

    /// <summary>
    /// 重新生成作物（树木、石头、杂草）
    /// </summary>
    private void CreateCrop()
    {
        //获得当前作物的位置
        Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);
        
        if (seedID != 0)
        {
            var tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);
            //生成树木的位置可能没有Dig瓦片
            if (tile == null)
            {
                tile = new TileDetails();
            }
            //瓦片初始化
            tile.daysSinceWatered = -1;
            tile.seedItemID = seedID;
            tile.growthDays = growthDay;
            //如果tile为空，需要在字典中更新一下
            GridMapManager.Instance.UpdateTileDetails(tile);
        }
    }
    
    
}
