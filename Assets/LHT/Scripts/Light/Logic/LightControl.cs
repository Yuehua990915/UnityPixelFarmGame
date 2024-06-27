using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightControl : MonoBehaviour
{
    public LightData_SO globalLightData;
    public LightData_SO partLightData;
    public bool isGlobal;

    public Sprite lightOn;
    public Sprite lightOff;
    private SpriteRenderer currentSprite;
    
    private Light2D currentLight;
    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
        if (transform.parent.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
        {
            currentSprite = spriteRenderer;
        }
        
    }

    /// <summary>
    /// 根据 season 和 lightShift 索引数据，进行灯光切换
    /// </summary>
    /// <param name="season"></param>
    /// <param name="lightShift"></param>
    /// <param name="timeDifference"></param>
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        LightData_SO currentData;
        if (isGlobal)
            currentData = globalLightData;
        else
            currentData = partLightData;
        
        //获得灯光数据
        currentLightDetails = currentData.GetLightDetails(season, lightShift);

        if (lightOff != null || lightOn != null)
        {
            if (currentLightDetails.lightShift == LightShift.Day)
                currentSprite.sprite = lightOff;
            else
                currentSprite.sprite = lightOn;
        }
        
        //执行缓慢切换
        if (timeDifference < Settings.lightChangeDuration)
        {
            //颜色偏移：例如，颜色需要从 0 变换 到 100（用时25s）
            //但是当前只有15s的时间，所以 “当前起始颜色 = 0 + colorOffset”
            //15s时，颜色应该为 （100 - 0）* 15 / 25 = 60
            //所以颜色从60开始，在15s内均匀变化到100
            var colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference;
            currentLight.color += colorOffset;
            
            //颜色
            //getter：需要变化的目标
            DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor,
                Settings.lightChangeDuration - timeDifference);
            //光照强度
            DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightIntensity,
                Settings.lightChangeDuration - timeDifference);
        }
        //瞬间切换
        if (timeDifference >= Settings.lightChangeDuration)
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightIntensity;
        }
    }
}
