using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimeLineManager : Singleton<TimeLineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector currentDirector;
    
    public bool isFinished;
    
    private bool isPaused;
    
    private bool isDone;
    public bool IsDone
    {
        set => isDone = value;
    }

    private Queue<double> clipLengths = new Queue<double>();

    private double endTime;
    //设置玩家面朝状态
    private bool setPlayState;
    
    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
        GetCurrentClipLength();

        setPlayState = true;
        GetPlayTrackEndTime();
    }

    private void OnEnable()
    {
        currentDirector.stopped += OnDirectorStoped;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }



    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnDirectorStoped(PlayableDirector obj)
    {
        //清空队列
        clipLengths.Clear();
            
        setPlayState = false;
        //设置玩家朝向
        foreach (var anim in PlayerMove.Instance.animators)
        {
            anim.SetFloat("InputX", 0);
            anim.SetFloat("InputY", 1);
        }
        //恢复时间显示
        //TimeManager.Instance.gameClockPause = false;
        isFinished = true;
    }
    
    private void OnStartNewGameEvent(int obj)
    {
        startDirector.transform.parent.gameObject.SetActive(true);
        isFinished = false;
    }

    private void Update()
    {
        if (isPaused && Input.GetKeyDown(KeyCode.E) && isDone)
        {
            isPaused = false;
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
            if (clipLengths.Count != 0)
            {
                currentDirector.time += clipLengths.Dequeue();
            }
        }
        
    }

    /// <summary>
    /// 暂停TimeLine
    /// 不能使用director.Pause
    /// </summary>
    /// <param name="director"></param>
    public void PauseTimeLine(PlayableDirector director)
    {
        currentDirector = director;
        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        isPaused = true;
    }

    /// <summary>
    /// Director -> Track -> Clip
    /// </summary>
    private void GetCurrentClipLength()
    {
        var binding = currentDirector.playableAsset.outputs;
        foreach (var item in binding)
        {
            if (item.sourceObject.GetType().Name == "DialogueTrack")
            {
                var diaTrack = item.sourceObject as DialogueTrack;
                if (diaTrack != null)
                {
                    foreach (var clip in diaTrack.GetClips())
                    {
                        clipLengths.Enqueue(clip.duration);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 在该轨道播放完的时间
    /// </summary>
    private void GetPlayTrackEndTime()
    {
        //拿到track group
        TimelineAsset timelineAsset = currentDirector.playableAsset as TimelineAsset;
        var rootTracks = timelineAsset.GetRootTracks();
        
        foreach (var groupTrack in rootTracks)
        {
            if (groupTrack.name == "Player" && groupTrack != null)
            {
                endTime = Double.MinValue;
                foreach (var childTrack in groupTrack.GetChildTracks())
                {
                    foreach (var clip in childTrack.GetClips())
                    {
                        if (endTime < clip.end)
                        {
                            endTime = clip.end;
                        }
                    }
                }
            }
        }
    }
}