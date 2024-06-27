using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData_SO", menuName = "Quest/QuestData")]
public class QuestData_SO : ScriptableObject
{
    [System.Serializable]
    public class QuestRequire
    {
        public int itemID;
        public int currentAmount;
        public int requireAmount;
        public Sprite itemImg;
    }
    [System.Serializable]
    public class QuestReward
    {
        public int rewardID;
        public int rewardAmount;
        public Sprite rewardImg;
    }
    
    public string questName;
    [TextArea]
    public string description;

    //接取未完成
    public bool isStarted;
    //完成未提交
    public bool isCompleted;
    //任务结束
    public bool isFinished;

    public List<QuestRequire> questRequires = new List<QuestRequire>();
    public List<QuestReward> QuestRewards = new List<QuestReward>();
}