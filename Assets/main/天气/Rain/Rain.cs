using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    public float rainSpeed = 20f;
    //各个时间段，雨的颜色
    public Color moringColor,dayColor,noonColor,nightColor;
    
    private ParticleSystem rainSystem;
    private ParticleSystem rainPointSystem;
    void Start()
    {
        rainSystem = GetComponent<ParticleSystem>();
        rainPointSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// 雨开启
    /// </summary>
    public void RainPlay()
    {
        var emission = rainSystem.emission;
        emission.rateOverTime = rainSpeed;
    }

    /// <summary>
    /// 雨关闭
    /// </summary>
    public void RainStop()
    {
        var emission = rainSystem.emission;
        emission.rateOverTime = 0f;
    }

    /// <summary>
    /// 根据类型切换雨的颜色
    /// </summary>
    /// <param name="type">1是早晨，2是白天，3是日落，4是晚上</param>
    public void ChangeRainColor(int type)
    {
        var main = rainSystem.main;
        switch (type)
        {
            case 1:
                main = rainSystem.main;
                main.startColor = moringColor;
                main = rainPointSystem.main;
                main.startColor = moringColor;
                break;
            case 2:
                main = rainSystem.main;
                main.startColor = dayColor;
                main = rainPointSystem.main;
                main.startColor = dayColor;
                break;
            case 3:
                main = rainSystem.main;
                main.startColor = noonColor;
                main = rainPointSystem.main;
                main.startColor = noonColor;
                break;
            case 4:
                main = rainSystem.main;
                main.startColor = nightColor;
                main = rainPointSystem.main;
                main.startColor = nightColor;
                break;
        }
    }
}
