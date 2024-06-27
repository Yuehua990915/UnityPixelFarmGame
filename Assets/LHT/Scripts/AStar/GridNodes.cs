using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.AStar
{
    public class GridNodes
    {
        //整个场景的网格宽高
        private int height;
        private int width;
        private Node[,] gridNode;

        /// <summary>
        /// 构造函数初始化节点范围数组
        /// </summary>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        public GridNodes(int width, int height)
        {
            this.height = height;
            this.width = width;

            gridNode = new Node[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridNode[x, y] = new Node(new Vector2Int(x,y));
                }
            }
        }

        /// <summary>
        /// 通过坐标得到节点
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public Node GetGridNode(int xPos, int yPos)
        {
            if (xPos < width && yPos < height)
            {
                return gridNode[xPos, yPos];
            }
            Debug.Log("超出网格范围");
            return null;
        }
    }
}
