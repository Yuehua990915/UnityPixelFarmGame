using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;

[RequireComponent(typeof(DataGUID))]
public class TimeManager : Singleton<TimeManager>, ISaveable
{
    private int second, minute, hour, day, week, month, year;
    private Season season = Season.春;
    //计时器
    private float timer;
    //季节计时器，当月份超过3(变量==0）变更季节
    private int monthInSeason = 3;

    //事件暂停
    public bool gameClockPause = false;
    //计时器
    private float tikTime;

    private float timeDifference;
    
    public TimeSpan GameTime => new TimeSpan(hour, minute, second);
    
    private void OnEnable()
    {
        EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        NewGameTime();
    }

    private void OnUpdateGameStateEvent(GameState state)
    {
        if (state == GameState.Pause)
        {
            gameClockPause = true;
        }
        else
        {
            gameClockPause = false;
        }
    }

    private void OnAfterSceneLoadEvent()
    {
        StartCoroutine(WaitForFadeOut());
        EventHandler.CallDateEvent(hour, day, week, month, year,season);
        EventHandler.CallMinuteEvent(minute, hour, day, week, season);
        EventHandler.CallLightShiftChangeEvent(season, getCurrentLightShift(), timeDifference);
    }

    private void OnBeforeSceneUnLoadEvent()
    {
        gameClockPause = true;
    }

    private IEnumerator WaitForFadeOut()
    {
        yield return new WaitForSeconds(2f);
        gameClockPause = false;
    }
    
    private void Start()
    {
        //注册
        ISaveable saveable = this;
        saveable.RegisterSaveable();

        gameClockPause = true;
        //游戏开始时执行更新
        // //不能在NewGameTime中调用，因为事件注册是在OnEnable中的，在Awake之后
        // EventHandler.CallDateEvent(hour,day,week,month,year,season);
        // EventHandler.CallMinuteEvent(minute,hour,day,week,season);
        // //游戏开始时切换灯光
        // EventHandler.CallLightShiftChangeEvent(season,getCurrentLightShift(), timeDifference);
    }

    private void Update()
    {
        if (!TimeLineManager.Instance.isFinished)
        {
            gameClockPause = true;
        }
        else
        {
            gameClockPause = false;
        }
        
        if (!gameClockPause)
        {
            tikTime += Time.deltaTime;
            if (tikTime >= Settings.gameTimeScale)
            {
                tikTime -= Settings.gameTimeScale;
                UpdateGameTime();
            }
        }

        //测试，快进
        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            day++;
            EventHandler.CallGameDayEvent(day,season);
            EventHandler.CallDateEvent(hour,day,week,month,year,season);
        }
    }

    void NewGameTime()
    {
        second = 0;
        minute = 0;
        hour = 7;
        day = 1;
        week = 1;
        month = 1;
        year = 1;
        season = Season.春;
    }

    void UpdateGameTime()
    {
        second++;
        if (second > Settings.SecondHold)
        {
            minute++;
            second = 0;
            if (minute > Settings.MinuteHold)
            {
                hour++;
                minute = 0;
                if (hour > Settings.HourHold)
                {
                    day++;
                    week++;
                    hour = 0;
                    if (week > 7)
                    {
                        week = 1;
                    }

                    if (day > Settings.DayHold)
                    {
                        month++;
                        day = 1;
                        if (month > 12)
                        {
                            month = 1;
                        }
                        //月份增加时，统计是否需要变更季节
                        monthInSeason--;
                        if (monthInSeason == 0)
                        {
                            monthInSeason = 3;
                            //Season为枚举，设置临时变量计算季节
                            int seasonNumber = (int)season;
                            seasonNumber++;
                            if (seasonNumber > Settings.SeasonHold)
                            {
                                seasonNumber = 0;
                                year++;
                            }
                            season = (Season)seasonNumber;
                            
                            if (year > 9999)
                            {
                                year = 1;
                            }
                        }
                        EventHandler.CallGameDayEvent(day,season);
                    }
                }EventHandler.CallDateEvent(hour,day,week,month,year,season);
            }
            EventHandler.CallMinuteEvent(minute,hour,day,week,season);
            //每分钟调用切换灯光
            EventHandler.CallLightShiftChangeEvent(season,getCurrentLightShift(), timeDifference);
        }
    }
    
    private LightShift getCurrentLightShift()
    {
        //白天
        if (GameTime >= Settings.dayTime && GameTime < Settings.nightTime)
        {
            //拿到时间差值，当差值小于Settings.lightChangeDuration时，渐变显示灯光
            //差值大于时，直接变化至设置灯光值
            timeDifference = (float)(GameTime - Settings.dayTime).TotalMinutes;
            return LightShift.Day;
        }
        //黑夜
        if (GameTime < Settings.dayTime || GameTime >= Settings.nightTime)
        {
            timeDifference = Mathf.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
            return LightShift.Night;
        }

        return LightShift.Day;
    }

    public string GUID => GetComponent<DataGUID>().guid;
    public GameSaveData GenerateSaveData()
    {
        GameSaveData gameSaveData = new GameSaveData();
        gameSaveData.timeDic = new Dictionary<string, int>();
        
        gameSaveData.timeDic.Add("Year",year);
        gameSaveData.timeDic.Add("Season", (int)season);
        gameSaveData.timeDic.Add("Month",month);
        gameSaveData.timeDic.Add("Week",week);
        gameSaveData.timeDic.Add("Day",day);
        gameSaveData.timeDic.Add("Hour",hour);
        gameSaveData.timeDic.Add("Minute",minute);
        gameSaveData.timeDic.Add("Second",second);

        return gameSaveData;
    }

    public void RestoreData(GameSaveData gameSaveData)
    {
        year = gameSaveData.timeDic["Year"];
        season = (Season)gameSaveData.timeDic["Season"];
        month = gameSaveData.timeDic["Month"];
        week = gameSaveData.timeDic["Week"];
        day = gameSaveData.timeDic["Day"];
        hour = gameSaveData.timeDic["Hour"];
        minute = gameSaveData.timeDic["Minute"];
        second = gameSaveData.timeDic["Second"];
    }
}
