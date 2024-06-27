using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllQuestData_SO", menuName = "Quest/AllQuestData_SO")]
public class AllQuestData_SO : ScriptableObject
{
    public List<QuestData_SO> questDataList = new List<QuestData_SO>();

    public QuestData_SO GetQuestData(string questName)
    {
        return questDataList.Find(q => q.questName == questName);
    }
}