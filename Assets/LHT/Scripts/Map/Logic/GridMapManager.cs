using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    //由于GetTileDetailsOnMousePosition()是public的，
    //不希望CursorManager以外的类调用
    //使用单例
    [RequireComponent(typeof(DataGUID))]
    public class GridMapManager : Singleton<GridMapManager>, ISaveable
    {
        //string: 场景名+坐标
        //TileDetails: 瓦片属性
        private Dictionary<string, TileDetails> tileDetailsDic = new Dictionary<string, TileDetails>();

        private Dictionary<string, bool> firstSceneLoadDic = new Dictionary<string, bool>();
        
        public RuleTile digTile;
        
        public RuleTile waterTile;

        private Tilemap digTilemap;
        
        private Tilemap waterTilemap;
        
        private Grid currentGrid;
        
        [Header("地图信息")]
        public List<MapData_SO> mapDataList;

        private Season currentSeason;

        private List<ReapItem> itemsInRadius;
        
        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap;
        }
        
        private void Start()
        {
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            
            foreach (var mapData in mapDataList)
            {
                firstSceneLoadDic.Add(mapData.sceneName, true);
                InitTileDetailsDic(mapData);
            }
        }
        
        /// <summary>
        /// 每天调用
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;

            foreach (var tile in tileDetailsDic)
            {
                if (tile.Value.daysSinceWatered > -1)
                {
                    tile.Value.daysSinceWatered = -1;
                }

                if (tile.Value.daysSinceDig > -1)
                {
                    tile.Value.daysSinceDig++;
                }
                //挖坑超过5天没有种植，复原
                if (tile.Value.daysSinceDig > 5 && tile.Value.seedItemID == -1)
                {
                    tile.Value.daysSinceDig = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }
                if (tile.Value.seedItemID != -1)
                {
                    tile.Value.growthDays++;
                }
            }
            //更新数据：删除重建
            RefreshMap();
        }

        /// <summary>
        /// 创建字典
        /// 1.获得Key
        /// 2.获得tileDetails
        ///     i.如果Dic中没有：根据SO文件中的数据更新属性
        ///     ii.如果Dic中有：获取已有的数据，在此基础上更新新属性
        /// </summary>
        /// <param name="mapData"></param>
        private void InitTileDetailsDic(MapData_SO mapData)
        {
            //获得string-> 获得瓦片坐标 & 场景名，拼接
            foreach (var tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };
                //获得字典的key
                string key = tileDetails.gridX + "x" +
                             tileDetails.gridY + "y" +
                             mapData.sceneName;
                //如果字典中有该瓦片信息
                if (GetTileDetails(key) != null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDrop = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NpcObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }
                //更新
                if (GetTileDetails(key) != null)
                    tileDetailsDic[key] = tileDetails;
                //创建
                else
                    tileDetailsDic.Add(key,tileDetails);
            }
        }

        /// <summary>
        /// 根据key返回Dic中的瓦片信息
        /// </summary>
        /// <param name="key">x+y+sceneName</param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDic.ContainsKey(key))
            {
                return tileDetailsDic[key];
            }
            return null;
        }

        /// <summary>
        /// 根据鼠标网格坐标获得对应瓦片数据
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mousePos)
        {
            string key = mousePos.x + "x" +
                         mousePos.y + "y" +
                         SceneManager.GetActiveScene().name;
            //不能直接从Dic中找，有可能报空
            return GetTileDetails(key);
        }
        
        private void OnAfterSceneLoadEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();
            
            //DisplayMap(SceneManager.GetActiveScene().name);
            //判断是否是第一次加载场景
            if (firstSceneLoadDic[SceneManager.GetActiveScene().name])
            {
                //只有在第一次加载场景时才会初始化地图作物状态
                //不判断则会反复重置作物信息
                EventHandler.CallGenerateCropEvent();
                firstSceneLoadDic[SceneManager.GetActiveScene().name] = false;
            }
            RefreshMap();
        }

        /// <summary>
        /// 执行实际工具或物品功能
        /// 不需要额外判断距离、位置等
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <param name="itemDetails"></param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            //获得鼠标点按的Grid Position
            var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            //获得鼠标当前点击的格子
            var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

            if (currentTile != null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);

                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemInScene(itemDetails.itemID,mouseWorldPos,itemDetails.itemType);
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;
                    case ItemType.Commodity:
                        EventHandler.CallDropItemInScene(itemDetails.itemID,mouseWorldPos, ItemType.Commodity);
                        break;
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        currentTile.daysSinceDig = 0;
                        currentTile.canDig = false;
                        currentTile.canDrop = false;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daysSinceWatered = 0;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
                        //传入Crop对应地图
                        //非空
                        currentCrop?.ProcessToolAction(itemDetails,currentCrop.tileDetails);
                        break;
                    case ItemType.CollectTool:
                        //执行收割方法
                        //传入当前鼠标点击地图
                        currentCrop.ProcessToolAction(itemDetails, currentTile);
                        break;
                    case ItemType.ReapTool:
                        var reapCount = 0;
                        for (int i = 0; i < itemsInRadius.Count; i++) 
                        {
                            //执行割草特效
                            EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, itemsInRadius[i].transform.position + Vector3.up);
                            //实现收获
                            itemsInRadius[i].SpawnHarvestItems();
                            //割草后，在列表中销毁
                            Destroy(itemsInRadius[i].gameObject);
                            
                            reapCount++;
                            //限制割草数量
                            if (reapCount >= Settings.reapCount)
                                break;
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;
                    case ItemType.Furniture:
                        //当场景切换时，保存家具（Item Manager）
                        //移除图纸（Inventory Manager）
                        //移除建造资源（Inventory Manager）
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID, mouseWorldPos);
                        break;
                }
                UpdateTileDetails(currentTile);
            }
        }

        /// <summary>
        /// 通过物理方法获得鼠标点击位置的作物
        ///Physics2D.OverlapPointAll()
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            //检测一个点，返回周围的碰撞体
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);

            Crop currentCrop = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>())
                {
                    currentCrop = colliders[i].GetComponent<Crop>();
                }
            }
            return currentCrop;
        }
        
        /// <summary>
        /// 鼠标点击、检测稻草组件，OverlapCircleNonAlloc，将稻草碰撞体组件传入列表
        /// 注意：Input得到的mousepos是屏幕坐标，需要得到世界坐标
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="mouseWorldPos">世界坐标</param>
        /// <returns></returns>
        public bool HaveReapableItemInRadius(ItemDetails tool, Vector3 mouseWorldPos)
        {
            itemsInRadius = new List<ReapItem>();
            Collider2D[] colliders = new Collider2D[20];
            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            var item = colliders[i].GetComponent<ReapItem>();
                            itemsInRadius.Add(item);
                        }
                    }
                }
            }
            return itemsInRadius.Count > 0;
        }

        /// <summary>
        /// 显示挖坑瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            //确定dig层不为空
            if (digTilemap != null)
            {
                digTilemap.SetTile(pos, digTile);
            }
        }
        
        /// <summary>
        /// 显示浇水瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            //确定dig层不为空
            if (waterTilemap != null)
            {
                waterTilemap.SetTile(pos, waterTile);
            }
        }

        /// <summary>
        /// 更新挖坑、浇水瓦片数据
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            //获得Key
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if (tileDetailsDic.ContainsKey(key))
            {
                tileDetailsDic[key] = tileDetails;
            }
            else
            {
                tileDetailsDic.Add(key, tileDetails);
            }
        }

        /// <summary>
        /// 显示地图瓦片
        /// </summary>
        /// <param name="sceneName"></param>
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDic)
            {
                var key = tile.Key;
                var tileDetails = tile.Value;

                if (key.Contains(sceneName))
                {
                    if (tileDetails.daysSinceDig > -1)
                    {
                        SetDigGround(tileDetails);
                    }

                    if (tileDetails.daysSinceWatered > -1)
                    {
                        SetWaterGround(tileDetails);
                    }
                    if (tileDetails.seedItemID > -1)
                    {
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID, tileDetails);
                    }
                }
            }
        }

        /// <summary>
        /// 刷新地图，删除瓦片和农作物，重新生成
        /// </summary>
        private void RefreshMap()
        {
            //瓦片
            if (digTilemap != null)
            {
                digTilemap.ClearAllTiles();
            }
            if (waterTilemap != null)
            {
                waterTilemap.ClearAllTiles();
            }
            //种子
            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            
            DisplayMap(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 根据场景名构建网格范围、输出范围和原点
        /// </summary>
        /// <param name="sceneName">场面名</param>
        /// <param name="gridDimension">网格范围</param>
        /// <param name="origin">网格原点</param>
        /// <returns>是否有当前场景信息</returns>
        public bool GetGridDimensions(string sceneName,out Vector2Int gridDimension,out Vector2Int origin)
        {
            gridDimension = Vector2Int.zero;
            origin = Vector2Int.zero;
            
            //遍历地图信息列表，找到当前场景信息，得到地图大小和地图原点
            foreach (var mapData in mapDataList)
            {
                if (mapData.sceneName == sceneName)
                {
                    gridDimension.x = mapData.gridWidth;
                    gridDimension.y = mapData.girdHeight;
                    origin.x = mapData.originX;
                    origin.y = mapData.originY;

                    return true;
                }
            }
            return false;
        }

        public string GUID => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData gameSaveData = new GameSaveData();
            gameSaveData.tileDetailsDic = tileDetailsDic;
            gameSaveData.firstSceneLoadDic = firstSceneLoadDic;

            return gameSaveData;
        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            tileDetailsDic = gameSaveData.tileDetailsDic;
            firstSceneLoadDic = gameSaveData.firstSceneLoadDic;
            
            RefreshMap();
        }
    }
}