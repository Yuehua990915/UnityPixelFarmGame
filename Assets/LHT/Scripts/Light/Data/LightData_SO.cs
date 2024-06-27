using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LightData_SO", menuName = "LightData/LightData_SO")]
public class LightData_SO : ScriptableObject
{
    public List<LightDetails> lightPattenList;
    
    /// <summary>
    /// 查找光照信息
    /// </summary>
    /// <returns></returns>
    public LightDetails GetLightDetails(Season season, LightShift lightShift)
    {
        return lightPattenList.Find(l => l.season == season && l.lightShift == lightShift);
    }
}

[System.Serializable]
public class LightDetails
{
    //光照颜色、光照强度、时间段、季节
    public Color lightColor;
    public float lightIntensity;
    public LightShift lightShift;
    public Season season;
}