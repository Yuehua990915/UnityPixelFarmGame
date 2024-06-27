using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneRouteDataList_SO", menuName = "Map/SceneRouteData")]
public class SceneRouteDataList_SO : ScriptableObject
{
    public List<SceneRoute> sceneRouteList;
}