using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueClip : PlayableAsset, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.None;
    public DialogueBehaviour dialogue = new DialogueBehaviour();
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        //创建自定义可播放项 ScriptPlayable<T>
        //T 必须继承自 PlayableBehaviour
        Playable playable = ScriptPlayable<DialogueBehaviour>.Create(graph, dialogue);
        return playable;
    }
}
