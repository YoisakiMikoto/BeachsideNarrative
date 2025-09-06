using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Mixer")]
    public AudioMixer masterMixer;
    
    [Header("Audio Parameters")]
    public string bgmVolumeParam = "BGM_Volume";
    public string envVolumeParam = "Env_Volume";
    
    [Header("Persistent Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource envSource;
    
    // PlayerPrefs存储键
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string ENV_VOLUME_KEY = "EnvVolume";
    
    // 音量范围
    private const float MIN_DB = -80f;
    private const float MAX_DB = 0f;

    // 当前音量（0-1范围）
    public float BGMVolume { get; private set; } = 0.7f;
    public float EnvVolume { get; private set; } = 0.8f;
    
    // 当前播放状态
    private bool isBgmPlaying = false;
    private bool isEnvPlaying = false;

    void Awake()
    {
        // 单例处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVolumes();
            
            // 确保AudioSource在场景切换时保持
            if (bgmSource != null) DontDestroyOnLoad(bgmSource.gameObject);
            if (envSource != null) DontDestroyOnLoad(envSource.gameObject);
            
            // 监听场景加载事件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        Debug.Log("AudioManager初始化完成");
    }
    
    void OnDestroy()
    {
        // 取消事件监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景加载: {scene.name}");
        // 确保音频继续播放
        if (isBgmPlaying && bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
        if (isEnvPlaying && envSource != null && !envSource.isPlaying)
        {
            envSource.Play();
        }
    }

    private void InitializeVolumes()
    {
        // 加载保存的音量
        BGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.7f);
        EnvVolume = PlayerPrefs.GetFloat(ENV_VOLUME_KEY, 0.8f);
        
        // 应用音量设置
        ApplyBGMVolume();
        ApplyEnvVolume();
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = Mathf.Clamp01(volume);
        ApplyBGMVolume();
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
    }

    public void SetEnvVolume(float volume)
    {
        EnvVolume = Mathf.Clamp01(volume);
        ApplyEnvVolume();
        PlayerPrefs.SetFloat(ENV_VOLUME_KEY, EnvVolume);
    }

    private void ApplyBGMVolume()
    {
        if (masterMixer == null || string.IsNullOrEmpty(bgmVolumeParam)) 
            return;
        
        float dbVolume = (BGMVolume <= 0.0001f) ? MIN_DB : Mathf.Log10(BGMVolume) * 20f;
        masterMixer.SetFloat(bgmVolumeParam, dbVolume);
    }

    private void ApplyEnvVolume()
    {
        if (masterMixer == null || string.IsNullOrEmpty(envVolumeParam)) 
            return;
        
        float dbVolume = (EnvVolume <= 0.0001f) ? MIN_DB : Mathf.Log10(EnvVolume) * 20f;
        masterMixer.SetFloat(envVolumeParam, dbVolume);
    }
    
    // 开始播放BGM
    public void StartBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Play();
            isBgmPlaying = true;
        }
    }
    
    // 开始播放环境音
    public void StartEnvironment()
    {
        if (envSource != null)
        {
            envSource.Play();
            isEnvPlaying = true;
        }
    }
    
    // 获取BGM源
    public AudioSource GetBGMSource()
    {
        return bgmSource;
    }
    
    // 获取环境音源
    public AudioSource GetEnvSource()
    {
        return envSource;
    }
}