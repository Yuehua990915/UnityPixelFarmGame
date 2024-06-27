using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ItemDetails
{
    //物品ID，名称
    public int itemID;
    public string itemName;
    
    //enums中的枚举类型
    //物品种类
    public ItemType itemType;
    
    //物品图标，在地图的上显示
    public Sprite icon;
    public Sprite itemOnWorldSprite;
    //物品使用范围
    public int itemUseRadius;
    //物品价格，出售折扣
    public int itemPrice;
    [Range(0,1)]
    public float salePercentage;
    //物品信息
    public string itemDetail;
    //状态：是否可以丢弃、捡起、携带、出售
    public bool canDropped;
    public bool canPicked;
    public bool canCarried;
    //public bool canSale;
}

/// <summary>
/// 此处使用结构体而不是类的原因
/// 1.以ID为例
/// 类需要赋初值，不赋值则为null，在背包中做判断会出现很多麻烦
/// 结构体不赋值，ID默认为0，此时当作判断条件会方便很多，只需要判断ID是否为0/小于多少即可
/// 2.以Amount为例
/// 类：当Amount为0，需要删除，删除可能导致背包排序出问题
/// 结构体：直接清零即可 
/// </summary>
[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
}

[System.Serializable]
public class AnimatorType
{
    public PartType partType;
    public PartName partName;
    public AnimatorOverrideController overrideController;
}

[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 pos;
}

[System.Serializable]
public class SceneFurniture
{
    public int itemID;
    public SerializableVector3 pos;
    public int boxIndex;
}

[System.Serializable]
public class TileProperty
{
    //坐标
    public Vector2Int tileCoordinate;
    //瓦片类型
    public GridType gridType;
    //当该瓦片有类型时，赋值为true，方便识别，方便赋值
    public bool boolTypeValue;
}

public class TileDetails
{
    public int gridX, gridY;
    public bool canDig;
    public bool canDrop;
    public bool canPlaceFurniture;
    public bool isNPCObstacle;
    public int daysSinceDig = -1;
    public int daysSinceWatered = -1;
    public int seedItemID = -1;
    public int growthDays = -1;
    public int daySinceLastHarvest = -1;
}

[System.Serializable]
public class NPCPosition
{
    public Transform npc;
    //可以使用[SceneName],但是Unity显示有Bug
    public string startScene;
    public Vector3 position;
}

//npc跨单个场景移动的路径
[System.Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}

//跨多个场景的路径list
//例如：从01-03场景，要走四条路线（01-02，02-03）
[System.Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}