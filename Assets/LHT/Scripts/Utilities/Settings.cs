using System;
using UnityEngine;

public class Settings
{
    //颜色变化所需时间
    public const float itemFadeDuration = 0.35f;
    //目标透明度
    public const float targetFade = 0.45f;
    //时间缩放
    public const float gameTimeScale = 0.01f;
    //设计时间阈值
    public const int SecondHold = 59;
    public const int MinuteHold = 59;
    public const int HourHold = 23;
    public const int DayHold = 30;
    public const int SeasonHold = 3;
    
    public const int WeekHold = 7;

    public const float fadeDuration = 1.5f;

    //割草数量限制
    public const int reapCount = 2;

    //NPC网格移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;

    //网格中每个像素的大小
    //一个网格为20*20
    // 1 / 20 = 0.05
    public const float pixelSize = 0.05f;

    public const float animationBreakTime = 5f;

    //用于判断SO中的99999坐标 大于该坐标
    public const int maxGridPos = 9999;


    //灯光渐变时间
    public const float lightChangeDuration = 25f;
    public static TimeSpan dayTime = new TimeSpan(6, 45, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);

    public static Vector3 newGamePlayerPos = new Vector3(10.5f, -6.5f, 0);
    public const int newGamePlayerMoney = 500;
}
