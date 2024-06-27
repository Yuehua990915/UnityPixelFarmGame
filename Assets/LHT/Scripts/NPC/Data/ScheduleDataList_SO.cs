using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScheduleDataList_SO", menuName = "NPC Schedule/ScheduleDataList")]
public class ScheduleDataList_SO : ScriptableObject
{
    public List<ScheduleDetails> scheduleList;

    public ScheduleDetails GetSchedule(int id)
    {
        return scheduleList.Find(m => m.id == id);
    }
}