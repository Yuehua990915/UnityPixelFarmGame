using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tree
{
    public Season season;
    public List<Node> nodeList;
    [HideInInspector]public Dictionary<string, Node> nodeDic = new Dictionary<string, Node>();
    
}
