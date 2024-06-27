using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialogue_SO", menuName = "Dialogue/DialogueData")]
public class Dialogue_SO : ScriptableObject
{
    public List<Tree> treeList;

#if UNITY_EDITOR
    //编辑器中执行更改时调用
    private void OnValidate()
    {
        foreach (var tree in treeList)
        {
            tree.nodeDic.Clear();
            foreach (var dialoguePiece in tree.nodeList)
            {
                if (!tree.nodeDic.ContainsKey(dialoguePiece.ID))
                {
                    tree.nodeDic.Add(dialoguePiece.ID,dialoguePiece);
                }
            }
        }
    }
#endif
}