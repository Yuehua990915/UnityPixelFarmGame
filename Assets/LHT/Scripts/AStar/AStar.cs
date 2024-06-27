using System.Collections.Generic;
using Farm.Map;
using UnityEngine;

namespace Farm.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;
        private Node startNode;
        private Node endNode;

        private int gridWidth;
        private int gridHeight;
        private int originX;
        private int originY;

        //存放当前选中的Node周围的点
        private List<Node> openNodeList;

        //存放所有被选中的点
        //使用HashSet是因为：1.不重复 2.查找速度快
        private HashSet<Node> closedNodeList;

        private bool pathFind;

        public void BulidPath(string sceneName, Vector2Int startPos, Vector2Int endPos,
            Stack<MovementStep> npcMovementStack)
        {
            pathFind = false;
            //网格信息创建成功
            if (GenerateGridNode(sceneName, startPos, endPos))
            {
                //查找最短路径
                if (FindShortestPath())
                {
                    //构建NPC移动路径
                    //NPC需要在固定的事件点到达某个位置
                    // 距离 % 速度 = 时间
                    //由于要倒推，所以先进后出，使用堆栈
                    UpdatePathOnMovementStepStack(sceneName, npcMovementStack);
                }
            }
        }

        /// <summary>
        /// 构建网格节点信息，初始化两个列表
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        private bool GenerateGridNode(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            //判断是否有当前场景信息
            if (GridMapManager.Instance.GetGridDimensions(sceneName, out Vector2Int gridDimension,
                    out Vector2Int origin))
            {
                gridNodes = new GridNodes(gridDimension.x, gridDimension.y);
                //赋值
                gridWidth = gridDimension.x;
                gridHeight = gridDimension.y;
                originX = origin.x;
                originY = origin.y;
                //初始化列表
                openNodeList = new List<Node>();
                closedNodeList = new HashSet<Node>();
            }
            else
                return false;

            //根据得到的原点创建起点、终点的节点
            startNode = gridNodes.GetGridNode(startPos.x - originX, startPos.y - originY);
            endNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);

            //通过key得到瓦片信息，判断是否是障碍物
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    //在字典中查找时，瓦片坐标都是没有经过原点处理的，是有负数的
                    //因为需要进行循环，所以x、y全为正数
                    //因此需要复原x、y，才能得到对应的瓦片坐标
                    Vector3Int tilePos = new Vector3Int(x + originX, y + originY, 0);
                    string key = tilePos.x + "x" + tilePos.y + "y" + sceneName;
                    TileDetails tile = GridMapManager.Instance.GetTileDetails(key);

                    if (tile != null)
                    {
                        //要将节点中的障碍物信息和瓦片中同步
                        //需要拿到当前节点
                        Node node = gridNodes.GetGridNode(x, y);
                        //node.isObstacle = tile.isNPCObstacle;
                        if (tile.isNPCObstacle)
                            node.isObstacle = true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 找到最短路径的所以Node，添加到ClosedNodeList
        /// </summary>
        /// <returns></returns>
        private bool FindShortestPath()
        {
            //添加起点
            openNodeList.Add(startNode);
            while (openNodeList.Count > 0)
            {
                //对列表排序
                //因为Node继承了IComparable
                //Sort会自动调用CompareTo()，找到最小的fCost/hCost
                openNodeList.Sort();
                //找到最近的节点
                Node closeNode = openNodeList[0];
                //移除、并加入closedNodeList
                openNodeList.RemoveAt(0);
                closedNodeList.Add(closeNode);

                //判断是否到达终点
                if (closeNode == endNode)
                {
                    pathFind = true;
                    break;
                }

                //计算周围的8个点，添加到OpenList
                EvaluateNeighbourNodes(closeNode);
            }

            return pathFind;
        }

        /// <summary>
        /// 评估周围8个点，并生成对应消耗值
        /// </summary>
        /// <param name="currentNode">目标节点</param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            Vector2Int currentNodePos = currentNode.gridPos;
            //可行的节点
            Node validNeighbourNode;
            //循环遍历周围8个节点
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    //不能直接使用gridNodes.GetGridNode
                    //如果遇到边界，会直接报空
                    //validNeighbourNode = gridNodes.GetGridNode(x, y);
                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y + y);
                    //遍历到的这一个相邻节点不为空
                    if (validNeighbourNode != null)
                    {
                        //判断当前节点是否进入OpenList
                        if (!openNodeList.Contains(validNeighbourNode))
                        {
                            //计算临近节点的gCost
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(validNeighbourNode, currentNode);
                            //计算临近节点的hCost
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, endNode);
                            //fCost自动进行计算

                            //新加入的节点链接父节点
                            validNeighbourNode.parentNode = currentNode;
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 找到有效的Node，非障碍、非选中
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x, int y)
        {
            //限定范围
            if (x >= gridWidth || y >= gridHeight || x < 0 || y < 0)
                return null;
            Node neighbourNode = gridNodes.GetGridNode(x, y);
            //判断是否是障碍物、判断是否已经被选中（进入closedList）
            if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
                return null;
            else
                return neighbourNode;
        }

        /// <summary>
        /// 获得两个节点之间的距离
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPos.x - nodeB.gridPos.x);
            int yDistance = Mathf.Abs(nodeA.gridPos.y - nodeB.gridPos.y);

            //x=y的格子内能走斜线，超出的部分只能走直线
            if (xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }

            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        /// <summary>
        /// 更新每一步坐标和场景名字
        /// 当到达指定的时间点时NPC会采取不同的行动，需要更新行动路径
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="movementStep"></param>
        private void UpdatePathOnMovementStepStack(string sceneName, Stack<MovementStep> npcMovementStep)
        {
            //第一个压入栈中的节点（终点）
            Node nextNode = endNode;
            //循环压栈，当到达startNode（没有parentNode）时结束
            while (nextNode != null)
            {
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPos.x + originX, nextNode.gridPos.y + originY);
                //入栈
                npcMovementStep.Push(newStep);
                //节点向前
                nextNode = nextNode.parentNode;
            }
        }
    }
}
