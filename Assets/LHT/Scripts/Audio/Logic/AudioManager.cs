using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : Singleton<AudioManager>
{
    public SoundDetailsList_SO soundData;
    public SceneSoundList_SO sceneSoundData;

    public AudioSource bgMusic;
    public AudioSource ambient;

    public float timeForBGMChange => Random.Range(4f, 8f);

    private Coroutine soundRoutine;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    [Header("Snapshot")]
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot ambientSnapshot;
    public AudioMixerSnapshot muteSnapshot;

    private float musicTransTime = 5f;
    
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
        }
        muteSnapshot.TransitionTo(1f);
    }

    private void OnPlaySoundEvent(SoundName soundName)
    {
        var soundDetails = soundData.GetSoundDetail(soundName);
        if (soundDetails != null)
        {
            EventHandler.CallInitSoundEffect(soundDetails);
        }
    }

    private void OnAfterSceneLoadEvent()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        //拿到当前场景的bgm和环境音名称
        var sceneSoundItem = sceneSoundData.GetSceneSoundItem(currentScene);

        //拿到音频数据
        SoundDetail bgmDetail = soundData.GetSoundDetail(sceneSoundItem.bgMusic);
        SoundDetail ambientDetail = soundData.GetSoundDetail(sceneSoundItem.ambient);
        //播放、切换音乐
        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
        }
        soundRoutine = StartCoroutine(PlaySoundRoutine(bgmDetail, ambientDetail));
    }

    private IEnumerator PlaySoundRoutine(SoundDetail bgm, SoundDetail ambient)
    {
        if (bgm != null && ambient != null)
        {
            //切换场景时，1s就开始播放环境音
            PlaySoundClip(ambient, false,1f);
            yield return new WaitForSeconds(timeForBGMChange);
            //过几秒再播放背景音
            PlaySoundClip(bgm, true, musicTransTime);
        }
    }
    
    private void PlaySoundClip(SoundDetail soundDetail, bool isBGM, float transitionTime)
    {
        if (isBGM)
        {
            bgMusic.clip = soundDetail.soundClip;
            audioMixer.SetFloat("MusicVolume", SetVolumeRegion(soundDetail.volume));
            if (bgMusic.isActiveAndEnabled)
            {
                bgMusic.Play();
            }
            //插值过渡到该快照
            normalSnapshot.TransitionTo(transitionTime);
        }
        else
        {
            ambient.clip = soundDetail.soundClip;
            audioMixer.SetFloat("AmbientVolume", SetVolumeRegion(soundDetail.volume));
            if (ambient.isActiveAndEnabled)
            {
                ambient.Play();
            }
            //插值过渡到该快照
            ambientSnapshot.TransitionTo(transitionTime);
        }
    }

    /// <summary>
    /// 将音量的区间从[0,1]改为[-80,20]
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    private float SetVolumeRegion(float volume)
    {
        return (volume * 100 - 80);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", SetVolumeRegion(volume));
    }
}
