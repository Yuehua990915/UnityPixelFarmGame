public enum ItemType
{
    //种子、货物、家具
    Seed,Commodity,Furniture,
    //锄头
    HoeTool,
    //斧头
    ChopTool,
    //锤子
    BreakTool,
    //镰刀
    ReapTool,
    //水壶
    WaterTool,
    //菜篮
    CollectTool,
    //可以被割的物体(杂草)
    ReapableScenery
}

/// <summary>
/// 格子类型
/// </summary>
public enum SlotType
{
    Bag,Box,Shop
}

/// <summary>
/// 拾取的物品所属位置
/// 玩家可以把物品放在背包/储物箱中
/// </summary>
public enum InventoryLocation
{
    Player,Box
    //Shop
}

/// <summary>
/// 不同的手持类型
/// 例如：举起、手持工具
/// </summary>
public enum PartType
{
    None,Carry,Hoe,Break,Water,Chop,Collect,Reap
}

/// <summary>
/// 用哪个部位来持有
/// </summary>
public enum PartName
{
    Body,Hair,Arm,Tool
}

public enum Season
{
    春,夏,秋,冬
}

public enum Week
{
    星期一,星期二,星期三,星期四,星期五,星期六,星期日
}

public enum GridType
{
    Diggable, DropItem, PlaceFurniture, NpcObstacle
}

//粒子特效类型
public enum ParticleEffectType
{
    None, LeavesFalling01,LeavesFalling02, Rock, ReapableScenery
}

public enum GameState
{
    GamePlay,Pause
}

public enum LightShift
{
    Day, Night
}

public enum SoundName
{
    //走路音效
    none, WalkOnSoft, WalkOnHard, WalkOnGrass,
    //工具音效
    Axe, Hoe, Reap, PickAxe, Water, Collect,
    //采集音效
    Plant, PickUp, TreeFalling, RockBroken,
    //环境音 & BGM
    AmbientCountryside1, AmbientCountryside2, AmbientInDoor, BGM1, BGM2,
    //TimeLineBGM
    TimeLineBGM, Thunder,
}