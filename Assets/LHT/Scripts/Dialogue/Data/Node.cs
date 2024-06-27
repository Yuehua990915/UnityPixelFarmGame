using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Node
{
    public string ID;
    public string name;
    public Sprite faceImg;

    [TextArea] public string text;

    public bool isLeft;
    [Header("结束对话")]
    public bool isEnd;
    [HideInInspector]public bool isDone;
    public bool hasPause;

    public QuestData_SO quest;
    public List<Option> optionList;
}

[System.Serializable]
public class Option
{
    public string targetID;
    public string text;
    public bool takeQuest;
}