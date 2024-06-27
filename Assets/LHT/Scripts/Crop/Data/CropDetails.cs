using UnityEngine;

[System.Serializable]
public class CropDetails
{
    public int seedItemID;

    [Header("每阶段生长时间")]
    //种子可能有几个生长阶段、种类不同每个阶段所需时间不同
    public int[] growthDays;

    //只读，计算完整生长周期
    public int TotalGrowthDay
    {
        get
        {
            int amount = 0;
            foreach (var days in growthDays)
            {
                amount += days;
            }

            return amount;
        }
    }

    [Header("不同生长阶段瓦片Prefab")] public GameObject[] growthPrefabs;
    [Header("不同阶段的图片")] public Sprite[] growthSprites;
    [Header("可种植季节")] public Season[] seasons;

    [Space] [Header("收割工具")] public int[] harvestToolItemID;
    [Header("每种工具使用次数")] public int[] requireActionCount;

    [Header("转换新物体ID")]
    //可反复收获的植物，在收获后，转换
    public int transferItemID;

    [Space] [Header("收割果实信息")] public int[] producedItemID;
    [Header("生成数量")] public int[] producedMinAmount;
    public int[] producedMaxAmount;
    [Header("生成范围")] public Vector2 spawnRadius;

    [Header("再次生长时间")] 
    public int daysToRegrow;
    [Header("可重复生长次数")]
    public int regrowTime;

    [Header("Options")] public bool generateAtPlayerPosition;

    //动画、粒子特效
    public bool hasAnim;

    public bool hasParticalEffect;
    //粒子特效类型
    public ParticleEffectType effectType;
    //特效位置
    public Vector3 effectPos;
    //音效
    public SoundName soundName;

    /// <summary>
    /// 检查当前工具是否可用
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public bool CheckToolAvailable(int toolID)
    {
        foreach (var tool in harvestToolItemID)
        {
            if (tool == toolID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 工具使用次数
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID)
    {
        for (int i = 0; i < harvestToolItemID.Length; i++)
        {
            if (harvestToolItemID[i] == toolID)
            {
                return requireActionCount[i];
            }
        }
        return -1;
    }
}