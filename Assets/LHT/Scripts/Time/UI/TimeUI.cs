using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    //圈 旋转
    public RectTransform timeCircle;
    //钟 激活
    public RectTransform clock;
    //日期 text
    public TextMeshProUGUI dateText;
    //时间 text
    public TextMeshProUGUI weekAndTimeText;
    //季节
    public Image seasonImg;
    //季节 图片变更
    public Sprite[] seasonList;

    public List<GameObject> clockList = new List<GameObject>();

    private TimeManager timeManager;

    private void Awake()
    {
        for (int i = 0; i < clock.childCount; i++)
        {
            clockList.Add(clock.GetChild(i).gameObject);
            clock.GetChild(i).gameObject.SetActive(false);
        }

        
    }

    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>().GetComponent<TimeManager>();
    }
    

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
    }
    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
    }
    
    private void OnGameMinuteEvent(int minute, int hour,int day, int week,Season season)
    {
        weekAndTimeText.text = "星期" + WeekTextChange(week) + " " + hour.ToString("00")+ ":" + minute.ToString("00");
    }
    private void OnGameDateEvent(int hour, int day, int week, int month, int year, Season season)
    {
        dateText.text = "第" + year.ToString() + "年" + month.ToString("00") + "月" + day.ToString("00") + "日";
        seasonImg.sprite = seasonList[(int)season];
        
        ShowClockBlocks(hour);
        RotateDayAndNightImg(hour);
    }

    /// <summary>
    /// 显示6块Clock图片
    /// </summary>
    /// <param name="hour"></param>
    void ShowClockBlocks(int hour)
    {
        //Clock有6块，相当于每4小时显示一块
        int index = hour / 4;
        
        if (index == 0)
        {
            foreach (var item in clockList)
            {
                item.gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < clockList.Count; i++)
            {
                //因为阈值的关系，hour不可能到24，所以要+1
                if (i < index + 1)
                {
                    clockList[i].SetActive(true);
                }
                else
                {
                    clockList[i].SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// 旋转图片，使用DOTWeen
    /// </summary>
    /// <param name="hour"></param>
    void RotateDayAndNightImg(int hour)
    {
        // 持续旋转
        // 让图片从黑夜开始：-90
        // var target = new Vector3(0, 0, hour * 15 - 90);
        // timeCircle.DORotate(target, 1f, RotateMode.Fast);

        //定时旋转
        if (hour == 7)
        {
            timeCircle.DORotate(new Vector3(0, 0, 0), 1f, RotateMode.Fast);
        }
        else if (hour == 10)
        {
            timeCircle.DORotate(new Vector3(0, 0, 90), 1f, RotateMode.Fast);
        }
        else if (hour == 16)
        {
            timeCircle.DORotate(new Vector3(0, 0, 180), 1f, RotateMode.Fast);
        }
        else if(hour == 19)
        {
            timeCircle.DORotate(new Vector3(0, 0, 270), 1f, RotateMode.Fast);
        }
    }
    

    /// <summary>
    /// 数字转汉字
    /// </summary>
    /// <param name="week"></param>
    /// <returns></returns>
    string WeekTextChange(int week)
    {
        switch (week)
        {
            case 1:
                return "一";
            case 2:
                return "二";
            case 3:
                return "三";
            case 4:
                return "四";
            case 5:
                return "五";
            case 6:
                return "六";
            case 7:
                return "日";
            default:
                return "";
        }
    }
    
}
