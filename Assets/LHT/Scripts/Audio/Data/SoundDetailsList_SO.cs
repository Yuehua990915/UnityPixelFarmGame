using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDetailsList_SO", menuName = "Sound/SoundDetailsList_SO")]
public class SoundDetailsList_SO : ScriptableObject
{
    public List<SoundDetail> soundDetailsList;

    public SoundDetail GetSoundDetail(SoundName soundName)
    {
        return soundDetailsList.Find(s => s.soundName == soundName);
    }
}

[System.Serializable]
public class SoundDetail
{
    public SoundName soundName;
    public AudioClip soundClip;
    [Range(0f, 1f)]
    public float volume;

    [Range(0.1f, 1.5f)]
    public float minPitch;
    [Range(0.1f, 1.5f)]
    public float maxPitch;
}