using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    private PlayableDirector director;
    public Node dialoguePiece;

    public override void OnPlayableCreate(Playable playable)
    {
        director = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        
        if (dialoguePiece.hasPause)
        {
            TimeLineManager.Instance.PauseTimeLine(director);
        }
        else
        {
            EventHandler.CallShowDialogueEvent(null);
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isPlaying)
        {
            TimeLineManager.Instance.IsDone = dialoguePiece.isDone;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(null);
    }

    public override void OnGraphStart(Playable playable)
    {
        //禁止移动
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        AudioSource[] audios =  Resources.FindObjectsOfTypeAll<AudioSource>();
        foreach (var audio in audios)
        {
            audio.gameObject.SetActive(false);
        }
    }

    public override void OnGraphStop(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
        AudioSource[] audios = Resources.FindObjectsOfTypeAll<AudioSource>();
        foreach (var audio in audios)
        {
            audio.gameObject.SetActive(true);
        }
    }
}
