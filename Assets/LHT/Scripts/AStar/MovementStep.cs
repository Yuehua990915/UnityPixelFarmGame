using UnityEngine;

namespace Farm.AStar
{
    /// <summary>
    /// NPC的每一步：对应场景、时间节点、坐标
    /// NPC是可以跨场景的
    /// </summary>
    public class MovementStep
    {
        public string sceneName;
        public int second;
        public int minute;
        public int hour;
        //步骤对应网格坐标
        public Vector2Int gridCoordinate;
    }
}