using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    [ExecuteInEditMode]
    public class GridMap : MonoBehaviour
    {
        public MapData_SO mapData;
        public GridType gridType;
        private Tilemap curTilemap;

        private void OnEnable()
        {
            //挂载脚本的物品没有运行时
            if (!Application.IsPlaying(this))
            {
                curTilemap = GetComponent<Tilemap>();
                //清空数据，在地图禁用前重新填入数据
                if (mapData != null)
                {
                    mapData.tileProperties.Clear();
                }
            }
        }

        private void OnDisable()
        {
            //挂载脚本的物品没有运行时
            if (!Application.IsPlaying(this))
            {
                curTilemap = GetComponent<Tilemap>();
                //更新数据
                UpdateTileProperties();

                //ScriptableObject的特性是临时保存，
                //要让其持久保存需要SetDirty
#if UNITY_EDITOR
                if (mapData != null)
                    EditorUtility.SetDirty(mapData);
#endif
            }
        }

        /// <summary>
        /// 更新瓦片信息到SO
        /// </summary>
        private void UpdateTileProperties()
        {
            //获取已经绘制的TileMap的边界
            //例如当前组件挂载在canDig上
            //拿到的是所有canDig地图的边界
            curTilemap.CompressBounds();

            if (!Application.IsPlaying(this))
            {
                //遍历x和y，获得所有瓦片的坐标
                if (mapData != null)
                {
                    //左下角的瓦片坐标
                    Vector3Int startPos = curTilemap.cellBounds.min;
                    //右上角的瓦片坐标
                    Vector3Int endPos = curTilemap.cellBounds.max;

                    for (int x = startPos.x; x < endPos.x; x++)
                    {
                        for (int y = startPos.y; y < endPos.y; y++)
                        {
                            //获得该类型地图的单一瓦片
                            TileBase tile = curTilemap.GetTile(new Vector3Int(x, y, 0));

                            //存入Data_SO
                            if (tile != null)
                            {
                                TileProperty newTile = new TileProperty
                                {
                                    tileCoordinate = new Vector2Int(x, y),
                                    gridType = this.gridType,
                                    boolTypeValue = true
                                };
                                //更新至SO
                                mapData.tileProperties.Add(newTile);
                            }
                        }
                    }
                }
            }
        }
    }
}