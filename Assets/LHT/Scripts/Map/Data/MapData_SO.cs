using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapData_SO", menuName = "Map/MapData")]
public class MapData_SO : ScriptableObject
{
    [SceneName]public string sceneName;
    [Header("地图范围")] 
    public int gridWidth;
    public int girdHeight;
    [Header("左下角原点坐标")] 
    public int originX;
    public int originY;
    public List<TileProperty> tileProperties;
}