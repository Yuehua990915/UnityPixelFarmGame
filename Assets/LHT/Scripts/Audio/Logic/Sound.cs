using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    //使用拖拽赋值，比Awake更快播放
    [SerializeField] private AudioSource audioSource;

    public void SetSound(SoundDetail soundDetail)
    {
        audioSource.clip = soundDetail.soundClip;
        audioSource.volume = soundDetail.volume;
        audioSource.pitch = Random.Range(soundDetail.minPitch, soundDetail.maxPitch);
    }
}
