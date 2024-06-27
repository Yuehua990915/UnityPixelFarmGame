using UnityEngine;

namespace Farm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;

        //父物体
        private Transform cropParent;

        //坐标
        private Grid currentGrid;

        private Season currentSeason;

        public bool isSeedAvailableInSeason;
        
        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }

        private void OnAfterSceneLoadEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            //获得种子信息
            CropDetails currentCrop = GetCropDetails(ID);
            isSeedAvailableInSeason = SeasonAvailable(currentCrop);
            //判断不为空，当前季节可以种植
            if (currentCrop != null && isSeedAvailableInSeason && tileDetails.seedItemID == -1)
            {
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
            //刷新地图
            else if (tileDetails.seedItemID != -1)
            {
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }

        /// <summary>
        /// 通过ID查找种子信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            return cropData.cropDetailsList.Find(c => c.seedItemID == ID);
        }

        /// <summary>
        /// 判断季节
        /// </summary>
        /// <param name="cropDetails"></param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails cropDetails)
        {
            for(int i = 0; i < cropDetails.seasons.Length; i++)
            {
                if (currentSeason == cropDetails.seasons[i])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 显示农作物
        /// </summary>
        /// <param name="tileDetails"></param>
        /// <param name="cropDetails"></param>
        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            int growthStage = cropDetails.growthDays.Length;
            int currentStage = 0;
            int daySum = cropDetails.TotalGrowthDay;
            //在切换场景和日期变更时会刷新场景
            //倒序计算当前成长阶段
            for (int i = growthStage - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= daySum)
                {
                    currentStage = i;
                    break;
                }

                daySum -= cropDetails.growthDays[i];
            }

            //获得当前阶段的Prefab
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSprites[currentStage];
            //tileDetails.gridXY坐标是在格子左下角，需要+0.5移动到中心位置
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);

            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;

            //当作物开始生长/只剩树桩/石头时，有碰撞
            BoxCollider2D[] collider2D = cropInstance.GetComponentsInChildren<BoxCollider2D>();
            if (currentStage != 0 || cropDetails.seedItemID == 1021 || cropDetails.seedItemID == 1022)
            {
                collider2D[collider2D.Length-1].enabled = true;
            }
            //树桩需要有碰撞体
            else if(cropDetails.seedItemID == 1019 || cropDetails.seedItemID == 1020)
            {
                collider2D[collider2D.Length-1].enabled = true;
            }
            else
            {
                collider2D[collider2D.Length-1].enabled = false;
            }
            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }
    }
}