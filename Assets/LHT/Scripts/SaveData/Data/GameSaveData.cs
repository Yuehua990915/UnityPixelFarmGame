using System.Collections.Generic;

public class GameSaveData
{
    //当前场景
    public string dataSceneName;
    //游戏中所有角色坐标
    public Dictionary<string, SerializableVector3> characterPosDic;
    //物品、家具
    public Dictionary<string, List<SceneItem>> sceneItemDic;
    public Dictionary<string, List<SceneFurniture>> sceneFurnitureDic;
    //背包、箱子
    public Dictionary<string, List<InventoryItem>> inventoryDic;
    //瓦片
    public Dictionary<string, TileDetails> tileDetailsDic;
    public Dictionary<string, bool> firstSceneLoadDic;
    //时间
    public Dictionary<string, int> timeDic;
    //金钱
    public int playerMoney;
    //NPC 当前路径（时间、目标场景、是否可互动，是否有动画剪辑）
    public string targetScene;
    public bool interactable;
    public int animationClipInstanceID;
    
    //任务信息
    public List<string> taskName;
    //状态
    public Dictionary<string, bool> taskStateDic;
}
