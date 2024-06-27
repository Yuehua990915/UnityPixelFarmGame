using UnityEngine;

public class LightManager : MonoBehaviour
{
    private LightControl[] sceneLight;
    private LightShift currentLightShift;
    private Season currentSeason;

    private float timeDifference = Settings.lightChangeDuration;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        currentLightShift = LightShift.Day;
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        currentSeason = season;
        this.timeDifference = timeDifference;
        if (currentLightShift != lightShift)
        {
            currentLightShift = lightShift;
            if (sceneLight == null)
            {
                sceneLight = FindObjectsOfType<LightControl>();
            }
            foreach (LightControl light in sceneLight)
            {
                //传入 season 和 lightShift
                light.ChangeLightShift(currentSeason, currentLightShift, this.timeDifference);
            }
        }
    }

    private void OnAfterSceneLoadEvent()
    {
        //拿到场景中所有灯光
        sceneLight = FindObjectsOfType<LightControl>();

        foreach (LightControl light in sceneLight)
        {
            //改变灯光
            light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);
        }
    }
}
