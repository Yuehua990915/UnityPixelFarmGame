using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneSoundList_SO", menuName = "Sound/SceneSoundList_SO")]
public class SceneSoundList_SO : ScriptableObject
{
    public List<SceneSoundItem> sceneSoundItemList;

    public SceneSoundItem GetSceneSoundItem(string sceneName)
    {
        return sceneSoundItemList.Find(s => s.sceneName == sceneName);
    }
}

[System.Serializable]
public class SceneSoundItem
{
    [SceneName] public string sceneName;
    public SoundName bgMusic;
    public SoundName ambient;
}

