using System.Collections;
using System.Collections.Generic;
using Farm.CropPlant;
using UnityEngine;

public class ReapItem : MonoBehaviour
{
    private CropDetails cropDetails;
    private Transform playerTrans => FindObjectOfType<PlayerMove>().transform;

    public void InitCropData(int id)
    {
        cropDetails = CropManager.Instance.GetCropDetails(id);
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
    }
}
