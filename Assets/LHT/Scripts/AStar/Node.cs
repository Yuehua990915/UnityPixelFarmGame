using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.AStar
{
    public class Node : IComparable<Node>
    {
        //网格坐标
        public Vector2Int gridPos;

        //到起点的距离
        public int gCost = 0;

        //到终点的距离
        public int hCost = 0;
        public int fCost => gCost + hCost;
        public bool isObstacle = false;
        public Node parentNode;

        public Node(Vector2Int pos)
        {
            gridPos = pos;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            //比较两个节点之间fCost
            //返回 -1， 0， 1
            int result = fCost.CompareTo(other.fCost);
            //当fCost相等，比较hCost的值
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }

            return result;
        }
    }
}
