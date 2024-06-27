using System;
using UnityEngine;

[Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int id;
    public int hour, minute, day;
    //优先级，越小越优先
    public int priority;
    public Season season;
    public string targetScene;
    public Vector2Int targetGridPos;
    public AnimationClip clipAtStop;//NPC停止移动时播放anim
    public bool interactable;
    public bool onlyOnce;
    public bool isFinished;


    public ScheduleDetails(int hour, int minute, int day, int priority, Season season, string targetScene, Vector2Int targetGridPos, AnimationClip clipAtStop, bool interactable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.priority = priority;
        this.season = season;
        this.targetScene = targetScene;
        this.targetGridPos = targetGridPos;
        this.clipAtStop = clipAtStop;
        this.interactable = interactable;
    }
    

    public int Time => (hour * 100) + minute;
    
    /// <summary>
    /// 比较优先级
    /// </summary>
    /// <param name="other"></param>
    /// <returns>-1:返回当前Schedule， 1：返回other的Schedule</returns>
    public int CompareTo(ScheduleDetails other)
    {
        if (Time == other.Time)
        {
            if (priority > other.priority)
                return 1;
            else //不存在时间相同且优先级相同的情况
                return -1;
        }
        else if (Time > other.Time)
            return 1;
        else if (Time < other.Time)
            return -1;

        return 0;
    }
}
