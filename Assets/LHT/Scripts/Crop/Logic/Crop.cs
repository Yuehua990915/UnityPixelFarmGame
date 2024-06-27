using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    private int harvestActionCount;
    //获得地图信息，用来保存数据
    public TileDetails tileDetails;

    private Animator anim;

    public bool canHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDay;
    private Transform playerTrans => FindObjectOfType<PlayerMove>().transform;
    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        tileDetails = tile;
        //砍多少下
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

        //树的动画在子物体的Top中
        anim = GetComponentInChildren<Animator>();
        
        if (harvestActionCount < requireActionCount)
        {
            harvestActionCount++;
            //动画
            if (anim != null && cropDetails.hasAnim)
            {
                //执行向左/向右摇晃的动画
                //当玩家在左侧砍，向右摇晃。在右侧砍同理
                //在左侧
                if (playerTrans.position.x < transform.position.x)
                    //树向右侧摇晃
                    anim.SetTrigger("RotateRight");
                else
                    anim.SetTrigger("RotateLeft");
            }
            //粒子特效
            if (cropDetails.hasParticalEffect)
                EventHandler.CallParticleEffectEvent(cropDetails.effectType,transform.position+ cropDetails.effectPos);
            //收获、砍树等音效
            if (cropDetails.soundName != SoundName.none)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundName);
            }
        }

        if (harvestActionCount >= requireActionCount)
        {
            //判断是否为收获
            //农作物和树桩
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnim)
            {
                //生成作物
                SpawnHarvestItems();
            }
            //树木倒下
            else if(cropDetails.hasAnim)
            {
                if (playerTrans.position.x < transform.position.x)
                    //向右倒
                    anim.SetTrigger("DownRight");
                else
                    anim.SetTrigger("DownLeft");
                //音效
                if (cropDetails.soundName == SoundName.Axe)
                {
                    EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                }
                else if(cropDetails.soundName == SoundName.PickAxe)
                {
                    EventHandler.CallPlaySoundEvent(SoundName.RockBroken);
                }
                
                //出现种子和木头
                StartCoroutine(HarvestAfterAnimation());
            }
        }
    }

    private IEnumerator HarvestAfterAnimation()
    {
        //只有动画在进入END状态时才执行后续步骤
        //参数：层数，没有叠层，所以为0
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("END"))
        {
            yield return 0;
        }
        
        //生成木头和种子
        SpawnHarvestItems();
        
        //转换为树桩
        if (cropDetails.transferItemID > 0)
        {
            CreateTransferCrop(); 
        }
    }

    private void CreateTransferCrop()
    {
        //设置瓦片seedID
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daySinceLastHarvest = -1;
        tileDetails.growthDays = 0;
        //刷新地图
        EventHandler.CallRefreshCurrentMap();
    }

    /// <summary>
    /// 生成农作物
    /// </summary>
    public void SpawnHarvestItems()
    {
        //遍历这个种子可以生成的所有物品，根据对应的生成数量进行生成
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            //作物数量
            int amount;
            //判断作物生成数量是否是随机的
            if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
            {
                amount = cropDetails.producedMinAmount[i];
            }
            else
            {
                amount = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i]);
            }
            
            //执行生成方法
            for (int j = 0; j < amount; j++)
            {
                //判断是收获还是砍伐
                if (cropDetails.generateAtPlayerPosition)
                {
                    //收获，在InventoryManager中注册方法，添加物品
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }
                else
                {
                    //砍伐，在地图上生成物品
                    //在玩家相对于树的另一侧，指定范围内生成
                    //获得方向
                    var dirx = transform.position.x > playerTrans.position.x ? 1 : -1;
                    //获得一个指定范围的随机坐标
                    var spawnPos = new Vector3(transform.position.x + Random.Range(dirx, cropDetails.spawnRadius.x * dirx),
                                               transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y),
                                               0);
                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                }
            }
        }

        if (tileDetails != null)
        {
            tileDetails.daySinceLastHarvest++;
            //判断是否能重复收获
            if(cropDetails.daysToRegrow > 0 && tileDetails.daySinceLastHarvest < cropDetails.regrowTime)
            {
                //能反复收获
                //重置收获时间
                tileDetails.growthDays = cropDetails.TotalGrowthDay - cropDetails.daysToRegrow;
                //刷新
                EventHandler.CallRefreshCurrentMap();
            }
            //不能重复收获，重置瓦片数据
            else
            {
                tileDetails.daySinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
            }
            Destroy(gameObject);
        }
    }
}
