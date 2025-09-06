using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Weather : MonoBehaviour
{
    private static readonly int SunDiscColor = Shader.PropertyToID("_SunDiscColor");
    
    [Serializable]
    public enum WeatherType
    {
        Sunny,Cloudy,Rain,None
    }

    [Serializable]
    public enum DayWeatherMode
    {
        Circle,Random
    }

    [Serializable]
    public class WeatherLightSatus
    {
        public WeatherType Type;
        [Header("晴天")]
        public float moringLight;
        public float dayLight;
        public float sunsetLight;
        public float nightLight;
        public Material daySkybox;       // 白天天空盒
        public Material nightSkybox;     // 夜晚天空盒
        public Material morningSkybox;   // 清晨天空盒
        public Material sunsetSkybox;    // 黄昏天空盒
    }

    [Serializable]
    public class DayWeather
    {
        public List<WeatherTime> weatherTimes;
    }

    [Serializable]
    public class WeatherTime
    {
        public WeatherType type;
        public float startTime;
    }
    
    [Header("昼夜切换")]
    public Light directionalLight;   // 绑定的方向光
    public float dayLength = 120f;   // 一整天时长（秒）
    
    public float fogDensityDay = 0.02f;
    public float fogDensityNight = 0.05f;
    
    //各种天气的光照参数 按照天气type顺序排，这样好找参数
    public List<WeatherLightSatus> LightStatus;

    public bool isMorning;
    public bool isDay;
    public bool isSunset;
    public bool isNight;
    public bool isChangeBox; //是否在切换天空盒

    public float time;              // 当前时间（0\~1）

    [Header("天气切换")] 
    public float weatherChangeTime; //天气切换的时间
    
    public bool isChangeWeather;//是否在切换天气
    public WeatherType weatherType;
    private WeatherType lastWeatherType;
    
    private float startTime;
    
    private Cloud cloud;
    
    [Header("天气预设")]
    [Tooltip("0-0.02清晨\n0.02-0.06切换白天 0.06-0.44白天\n0.44-0.48切换日落 0.48-0.5日落\n0.5-0.54切换夜晚 0.54-0.96夜晚\n0.96-1切换清晨")]
    public List<DayWeather> dayWeathers;
    public DayWeatherMode dayWeatherMode;//天气循环模式
    
    [SerializeField]
    private int dayWeatherIndex;
    
    [Header("雨粒子系统")]
    public Rain rain;
    void Start()
    {
        cloud = GetComponent<Cloud>();
        
        //整理顺序
        List<WeatherLightSatus> status = new List<WeatherLightSatus>();
        for (int i = 0; i < (int)WeatherType.None; i++)
        {
            for (int j = 0; j < LightStatus.Count; j++)
            {
                if (i == (int)LightStatus[j].Type)
                {
                    status.Add(LightStatus[j]);
                }
            }
        }
        LightStatus = status;

        lastWeatherType = weatherType;
        
        RenderSettings.skybox.Lerp(RenderSettings.skybox, LightStatus[0].morningSkybox, 1);
    }
    
    void Update()
    {
        // 更新时间（0\~1循环）
        time += Time.deltaTime / dayLength;
        if (time >= 1)
        {
            AdayGone();
            time %= 1;
        }
        
        DayLightChange();
        LightColorChange();
        Fog();

        isChangeWeather = CanChangeWeather();
        
        ChangeWeatherByTime();
    }

    //每天结束
    private void AdayGone()
    {
        ChangeWeatherByMode(dayWeatherMode);
    }
    
    private void ChangeWeatherByMode(DayWeatherMode mode)
    {
        //根据模式切换天气
        switch (mode)
        {
            case DayWeatherMode.Circle:
                dayWeatherIndex++;
                CheckIndex(ref dayWeatherIndex, dayWeathers.Count);
                break;
            case DayWeatherMode.Random:
                dayWeatherIndex = Random.Range(0, dayWeathers.Count);
                break;
        }
    }

    private void ChangeWeatherByTime()
    {
        for (int i = dayWeathers[dayWeatherIndex].weatherTimes.Count - 1; i >= 0; i--)
        {
            if (time > dayWeathers[dayWeatherIndex].weatherTimes[i].startTime)
            {
                if (weatherType != dayWeathers[dayWeatherIndex].weatherTimes[i].type)
                {
                    ChangeWeather(dayWeathers[dayWeatherIndex].weatherTimes[i].type);
                }

                return;
            }
        }
    }
    
    private void DayLightChange()
    {
        //0-0.02清晨
        //0.02-0.06切换白天 0.06-0.44白天
        //0.44-0.48切换日落 0.48-0.5日落
        //0.5-0.54切换夜晚 0.54-0.96夜晚
        //0.96-1切换清晨
        
        // 计算太阳角度（0\~360度）

        if (!isNight)
        {
            float sunAngle = time * 360f;
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        }
        else
        {
            float sunAngle = (time-0.54f) * 3480/7f-14.4f;
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        }

        // 调整光照强度（日出日落渐变）
        if (time <= 0.02f)//清晨
        {
            CheckIsChangeBox(false);
            Morning();
        }
        else if (time <= 0.06f)//切换白天
        {
            if (!isDay)
            {
                isMorning = false;
                isNight = false;
                isDay = true;
                isSunset = false;
                if(rain != null)
                    rain.ChangeRainColor(2);
            }
            CheckIsChangeBox(true);
            DayChange(0.02f,25);
        }
        else if (time <= 0.44f)//白天
        {
            CheckIsChangeBox(false);
            Day();
        }
        else if (time <= 0.48f) //切换日落
        {
            if (!isSunset)
            {
                isMorning = false;
                isNight = false;
                isDay = false;
                isSunset = true;
                if(rain != null) 
                    rain.ChangeRainColor(3);
            }
            CheckIsChangeBox(true);
            SunDownChange(0.44f,25);
        }  
        else if (time <= 0.5f)//日落
        {
            CheckIsChangeBox(false);
            Sunset();
        }
        else if (time <= 0.54f)//切换夜晚
        {
            if (!isNight)
            {
                isMorning = false;
                isNight = true;
                isDay = false;
                isSunset = false;
                if(rain != null) 
                    rain.ChangeRainColor(4);
            }
            CheckIsChangeBox(true);
            NightChange(0.5f,25);
        }
        else if (time <= 0.96f) //夜晚
        {
            CheckIsChangeBox(false);
            Night();
        }
        else//切换清晨
        {
            if (!isMorning)
            {
                isMorning = true;
                isNight = false;
                isDay = false;
                isSunset = false;
                if(rain != null) 
                    rain.ChangeRainColor(1);
            }
            CheckIsChangeBox(true);
            SunRiseChange(0.96f,25);
        }

        DynamicGI.UpdateEnvironment(); // 更新全局光照
    }

    #region 天空盒切换
    private void SunRiseChange(float timeStart,float grain)
    {
        RenderSettings.skybox.Lerp(LightStatus[(int)weatherType].nightSkybox,LightStatus[(int)weatherType].morningSkybox,(time-timeStart) * grain);
        LightDensityChangeTo(LightStatus[(int)weatherType].moringLight,(time-timeStart) * grain);
    }
    private void Morning()
    {
        if (weatherType != lastWeatherType)
        {
            if (WCCheck())
            {
                lastWeatherType = weatherType;
                isChangeWeather = false;
                return;
            }
            
            LightDensityChangeTo(LightStatus[(int)weatherType ].moringLight,(Time.time-startTime)/weatherChangeTime);
            RenderSettings.skybox.Lerp(LightStatus[(int)lastWeatherType ].morningSkybox, LightStatus[(int)weatherType ].morningSkybox, (Time.time-startTime)/weatherChangeTime);
        }
    }
    private void DayChange(float timeStart,float grain)
    {
        RenderSettings.skybox.Lerp(LightStatus[(int)weatherType].morningSkybox,LightStatus[(int)weatherType].daySkybox,(time-timeStart) * grain);
        LightDensityChangeTo(LightStatus[(int)weatherType].dayLight,(time-timeStart) * grain);
    }
    private void Day()
    {
        if (weatherType != lastWeatherType)
        {
            if (WCCheck())
            {
                lastWeatherType = weatherType;
                isChangeWeather = false;
                return;
            }
            
            LightDensityChangeTo(LightStatus[(int)weatherType ].dayLight,(Time.time-startTime)/weatherChangeTime);
            RenderSettings.skybox.Lerp(LightStatus[(int)lastWeatherType ].daySkybox, LightStatus[(int)weatherType ].daySkybox, (Time.time-startTime)/weatherChangeTime);
        }
    }
    private void SunDownChange(float timeStart,float grain)
    {
        RenderSettings.skybox.Lerp(LightStatus[(int)weatherType].daySkybox,LightStatus[(int)weatherType].sunsetSkybox,(time - timeStart) * grain);
        LightDensityChangeTo(LightStatus[(int)weatherType].sunsetLight,(time-timeStart) * grain);
    }
    private void Sunset()
    {
        if (weatherType != lastWeatherType)
        {
            if (WCCheck())
            {
                lastWeatherType = weatherType;
                isChangeWeather = false;
                return;
            }
            
            LightDensityChangeTo(LightStatus[(int)weatherType ].sunsetLight,(Time.time-startTime)/weatherChangeTime);
            RenderSettings.skybox.Lerp(LightStatus[(int)lastWeatherType ].sunsetSkybox, LightStatus[(int)weatherType ].sunsetSkybox, (Time.time-startTime)/weatherChangeTime);
        }
    }
    private void NightChange(float timeStart,float grain)
    {
        RenderSettings.skybox.Lerp(LightStatus[(int)weatherType].sunsetSkybox,LightStatus[(int)weatherType].nightSkybox,(time - timeStart) * grain);
        LightDensityChangeTo(LightStatus[(int)weatherType].nightLight,(time-timeStart) * grain);
    }
    private void Night()
    {
        if (weatherType != lastWeatherType)
        {
            if (WCCheck())
            {
                lastWeatherType = weatherType;
                isChangeWeather = false;
                return;
            }
            
            LightDensityChangeTo(LightStatus[(int)weatherType ].nightLight,(Time.time-startTime)/weatherChangeTime);
            RenderSettings.skybox.Lerp(LightStatus[(int)lastWeatherType ].nightSkybox, LightStatus[(int)weatherType ].nightSkybox, (Time.time-startTime)/weatherChangeTime);
        }
    }
    #endregion

    private void LightDensityChangeTo(float value,float t)
    {
        directionalLight.intensity = Mathf.Lerp(directionalLight.intensity,value,t);
    }
    private void LightColorChange()
    {
        directionalLight.color =RenderSettings.skybox.GetColor(SunDiscColor);
    }
    
    private void Fog()
    {
        RenderSettings.fogDensity = Mathf.Lerp(
            fogDensityDay, 
            fogDensityNight, 
            Mathf.Clamp01(Mathf.Abs(time - 0.5f) * 2) // 离正午越远雾越浓
        );
    }

    private void CheckIsChangeBox(bool p)
    {
        if (!p == isChangeBox)
        {
            isChangeBox = !isChangeBox;
        }
    }

    private bool WCCheck()
    {
        if (Time.time>=startTime+weatherChangeTime)
        {
            return true;
        }
        return false;
    }

    private bool CanChangeWeather()
    {
        float p = weatherChangeTime/dayLength;
        if (((0.02-p) <= time && time <= 0.06) || ((0.44-p) <= time && time <= 0.48) || ((0.5-p) <= time && time <= 0.54) ||
            ((0.96-p) <= time && time <= 1))
        {
            return true;
        }
        return false;
    }

    private void CheckIndex(ref int index,int maxIndex)
    {
        if (index >= maxIndex)
        {
            index = 0;
        }

        if (index < 0)
        {
            index = maxIndex-1;
        }
    }
    
    /// <summary>
    /// 更换天气
    /// </summary>
    /// <param name="t"></param>
    public void ChangeWeather(WeatherType t)
    {
        if (!isChangeBox)
        {
            if (!isChangeWeather)
            {
                weatherType = t;
                isChangeWeather = true;
                startTime=Time.time;
                
                if (lastWeatherType == WeatherType.Rain)
                {
                    rain.RainStop();
                    directionalLight.shadowStrength = 1f;
                }

                //云设置
                switch (t)
                {
                    case WeatherType.Sunny:
                        cloud.SetSpawnRate(1f);
                        break;
                    case WeatherType.Cloudy:
                        cloud.PrewarmClouds(10);
                        cloud.SetSpawnRate(2f);
                        break;
                    case WeatherType.Rain:
                        cloud.PrewarmClouds(20);
                        cloud.SetSpawnRate(3f);
                        rain.RainPlay();
                        directionalLight.shadowStrength = 0.5f;
                        break;
                }
            }
        }
    }
}
