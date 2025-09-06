using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GlobalAudioSystem : MonoBehaviour
{
    private static GlobalAudioSystem _instance;
    public static GlobalAudioSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GlobalAudioSystem>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GlobalAudioSystem");
                    _instance = obj.AddComponent<GlobalAudioSystem>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class AudioTrack
    {
        public string trackName;
        public AudioSource source;
        public AudioClip[] clips;

        [Header("Settings")]
        public float fadeInTime = 2f;
        public float fadeOutTime = 3f;
        public float minInterval = 10f;
        public float maxInterval = 15f;
        public bool loop = false;

        [Header("State")]
        public bool isPlaying = false;
        public bool isFading = false;
        public AudioClip currentClip;
        public float volumeMultiplier = 1f;
    }

    [System.Serializable]
    public class UITrack
    {
        public AudioSource source;
        public AudioClip[] clickSounds;
        public AudioClip[] hoverSounds;
        [Range(0f, 1f)] public float volume = 0.8f;
    }

    [Header("Audio Tracks")]
    public AudioTrack bgmTrack;
    public AudioTrack envTrack;

    [Header("UI Audio")]
    public UITrack uiTrack;
    public UITrack houseSetTrack;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool pauseOnSceneLoad = false;

    private Dictionary<AudioTrack, Coroutine> activeCoroutines = new Dictionary<AudioTrack, Coroutine>();
    private Scene currentScene;
    private bool isSwitchingScene = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 确保有音频源
        if (bgmTrack.source == null)
            bgmTrack.source = CreateAudioSource("BGM_Source");
        if (envTrack.source == null)
            envTrack.source = CreateAudioSource("ENV_Source");
        if (uiTrack.source == null)
            uiTrack.source = CreateAudioSource("UI_Sound_Source");
        if (houseSetTrack.source == null)
            houseSetTrack.source = CreateAudioSource("HouseSet_Sound_Source");

        // 配置音频源
        bgmTrack.source.loop = bgmTrack.loop;
        envTrack.source.loop = envTrack.loop;
        uiTrack.source.playOnAwake = false;
        uiTrack.source.loop = false;
        houseSetTrack.source.playOnAwake = false;
        houseSetTrack.source.loop = false;

        // 初始化场景
        currentScene = SceneManager.GetActiveScene();

        // 注册场景切换事件
        SceneManager.activeSceneChanged += OnSceneChanged;

        // 加载保存的音量设置
        LoadSavedVolumes();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    private void OnSceneChanged(Scene previousScene, Scene newScene)
    {
        if (showDebugInfo)
            Debug.Log($"[AudioSystem] 场景切换: {previousScene.name} -> {newScene.name}");

        isSwitchingScene = true;
        currentScene = newScene;

        // 处理场景切换时的音频行为
        if (pauseOnSceneLoad)
        {
            PauseAllAudio();
        }

        // 恢复音频状态
        StartCoroutine(PostSceneChangeRoutine());
    }

    private IEnumerator PostSceneChangeRoutine()
    {
        // 等待一帧确保场景加载完成
        yield return null;

        isSwitchingScene = false;

        // 恢复播放状态
        if (pauseOnSceneLoad)
        {
            ResumeAllAudio();
        }

        // 确保音频源正确重新初始化
        ReinitializeAudioSources();

        if (showDebugInfo)
            Debug.Log("[AudioSystem] 场景切换后音频系统恢复完成");
    }

    private void ReinitializeAudioSources()
    {
        // 确保音频源仍然有效
        if (bgmTrack.source == null)
            bgmTrack.source = CreateAudioSource("BGM_Source_Restored");
        if (envTrack.source == null)
            envTrack.source = CreateAudioSource("ENV_Source_Restored");
        if (uiTrack.source == null)
            uiTrack.source = CreateAudioSource("UI_Sound_Source_Restored");
            if (houseSetTrack.source == null)
                houseSetTrack.source = CreateAudioSource("HouseSet_Sound_Source_Restored");

        // 重新应用音量设置
        bgmTrack.source.volume = bgmTrack.volumeMultiplier;
        envTrack.source.volume = envTrack.volumeMultiplier;
        uiTrack.source.volume = uiTrack.volume;
        // 房屋设置音频源音量
        houseSetTrack.source.volume = envTrack.volumeMultiplier;

        // 恢复播放状态
        if (bgmTrack.isPlaying && !bgmTrack.source.isPlaying)
            bgmTrack.source.Play();
        if (envTrack.isPlaying && !envTrack.source.isPlaying)
            envTrack.source.Play();
    }

    void Start()
    {
        // 开始播放循环
        StartTrack(bgmTrack);
        StartTrack(envTrack);
    }

    private AudioSource CreateAudioSource(string name)
    {
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.SetParent(transform);
        AudioSource newSource = sourceObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        return newSource;
    }

    public void StartTrack(AudioTrack track)
    {
        if (track == null || track.source == null) return;

        // 如果已经在播放，先停止
        if (activeCoroutines.ContainsKey(track))
        {
            StopCoroutine(activeCoroutines[track]);
            activeCoroutines.Remove(track);
        }

        track.isPlaying = true;
        Coroutine routine = StartCoroutine(PlayTrackRoutine(track));
        activeCoroutines[track] = routine;
    }

    public void StopTrack(AudioTrack track, bool immediate = false)
    {
        if (track == null || !track.isPlaying) return;

        track.isPlaying = false;

        if (immediate)
        {
            if (activeCoroutines.ContainsKey(track))
            {
                StopCoroutine(activeCoroutines[track]);
                activeCoroutines.Remove(track);
            }
            track.source.Stop();
        }
    }

    public void PauseAllAudio()
    {
        if (bgmTrack.source != null && bgmTrack.source.isPlaying)
            bgmTrack.source.Pause();
        if (envTrack.source != null && envTrack.source.isPlaying)
            envTrack.source.Pause();
        if (uiTrack.source != null && uiTrack.source.isPlaying)
            uiTrack.source.Pause();
        if (houseSetTrack.source != null && houseSetTrack.source.isPlaying)
            houseSetTrack.source.Pause();
    }

    public void ResumeAllAudio()
    {
        if (bgmTrack.source != null && !bgmTrack.source.isPlaying)
            bgmTrack.source.UnPause();
        if (envTrack.source != null && !envTrack.source.isPlaying)
            envTrack.source.UnPause();
        if (uiTrack.source != null && !uiTrack.source.isPlaying)
            uiTrack.source.UnPause();
            if (houseSetTrack.source != null && !houseSetTrack.source.isPlaying)
                houseSetTrack.source.UnPause();
    }

    private IEnumerator PlayTrackRoutine(AudioTrack track)
    {
        while (track.isPlaying)
        {
            // 场景切换时暂停处理
            if (isSwitchingScene)
            {
                yield return new WaitWhile(() => isSwitchingScene);
            }

            // 随机选择音频
            if (track.clips == null || track.clips.Length == 0)
            {
                Debug.LogError($"[{track.trackName}] 没有可用的音频片段!");
                yield break;
            }

            AudioClip clip = track.clips[Random.Range(0, track.clips.Length)];
            track.currentClip = clip;

            if (showDebugInfo)
                Debug.Log($"[{track.trackName}] 开始播放: {clip.name} (长度: {clip.length}秒)");

            // 设置音频源
            track.source.clip = clip;
            track.source.volume = 0f; // 初始音量为0，淡入

            // 播放音频
            track.source.Play();

            // 淡入
            track.isFading = true;
            yield return FadeIn(track.source, track.fadeInTime, track);
            track.isFading = false;

            // 等待音频播放（减去淡出时间）
            float playTime = clip.length - track.fadeInTime - track.fadeOutTime;

            if (playTime > 0)
            {
                if (showDebugInfo)
                    Debug.Log($"[{track.trackName}] 主播放阶段: {playTime}秒");

                float timer = 0f;
                while (timer < playTime && track.isPlaying)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;

                    // 场景切换时暂停处理
                    if (isSwitchingScene)
                    {
                        yield return new WaitWhile(() => isSwitchingScene);
                    }
                }
            }

            // 淡出
            if (track.isPlaying && !track.loop) // 循环轨道不淡出
            {
                track.isFading = true;
                yield return FadeOut(track.source, track.fadeOutTime, track);
                track.isFading = false;
            }

            // 随机间隔
            if (track.isPlaying && !track.loop) // 循环轨道不等待间隔
            {
                float interval = Random.Range(track.minInterval, track.maxInterval);

                if (showDebugInfo)
                    Debug.Log($"[{track.trackName}] 等待间隔: {interval}秒");

                float timer = 0f;
                while (timer < interval && track.isPlaying)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;

                    // 场景切换时暂停处理
                    if (isSwitchingScene)
                    {
                        yield return new WaitWhile(() => isSwitchingScene);
                    }
                }
            }

            // 循环轨道特殊处理
            if (track.loop && track.isPlaying)
            {
                track.source.volume = track.volumeMultiplier;
                while (track.source.isPlaying && track.isPlaying)
                {
                    yield return null;

                    // 场景切换时暂停处理
                    if (isSwitchingScene)
                    {
                        yield return new WaitWhile(() => isSwitchingScene);
                    }
                }
            }
        }

        // 停止音频源
        if (track.source != null && track.source.isPlaying)
        {
            track.source.Stop();
        }

        activeCoroutines.Remove(track);
    }

    private IEnumerator FadeIn(AudioSource source, float duration, AudioTrack track)
    {
        if (source == null) yield break;

        float timer = 0f;
        float endVol = 0f;

        while (timer < duration)
        {
            if (source == null) yield break;

            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            source.volume = Mathf.Lerp(endVol, track.volumeMultiplier, t);
            yield return null;

            if (isSwitchingScene)
            {
                yield return new WaitWhile(() => isSwitchingScene);
            }
        }

        if (source != null)
        {
            source.volume = track.volumeMultiplier;
        }
    }

    private IEnumerator FadeOut(AudioSource source, float duration, AudioTrack track)
    {
        if (source == null) yield break;

        float timer = 0f;


        while (timer < duration)
        {
            if (source == null) yield break;

            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            source.volume = Mathf.Lerp(track.volumeMultiplier, 0f, t);
            yield return null;

            if (isSwitchingScene)
            {
                yield return new WaitWhile(() => isSwitchingScene);
            }
        }

        if (source != null)
        {
            source.volume = 0f;
        }
    }

    // 设置轨道音量（0-1范围）
    public void SetTrackVolume(AudioTrack track, float volume)
    {
        if (track == null) return;

        volume = Mathf.Clamp01(volume);
        track.volumeMultiplier = volume;

        if (!track.isFading && track.source != null)
        {
            track.source.volume = volume;
        }
    }

    // 设置UI音量
    public void SetUIVolume(float volume)
    {
        uiTrack.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("UI_Volume", uiTrack.volume);
        PlayerPrefs.Save();

        if (showDebugInfo)
            Debug.Log($"[UI音效] 音量设置: {uiTrack.volume}");
    }
    //
    public void SetHouseSetVolume(float volume)
    {
        houseSetTrack.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("HouseSet_Volume", houseSetTrack.volume);
        PlayerPrefs.Save();

        if (showDebugInfo)
            Debug.Log($"[房屋设置音效] 音量设置: {houseSetTrack.volume}");
    }

    // 播放UI点击音效
    public void PlayUIClick()
    {
        if (uiTrack.source == null || uiTrack.clickSounds == null || uiTrack.clickSounds.Length == 0)
            return;

        AudioClip clip = uiTrack.clickSounds[Random.Range(0, uiTrack.clickSounds.Length)];
        PlayUIEffect(clip);
    }
    //清脆
    public void PlayHouseSetClick()
    {
        if (houseSetTrack.source == null || houseSetTrack.clickSounds == null || houseSetTrack.clickSounds.Length == 0)
            return;

        AudioClip clip = houseSetTrack.clickSounds[Random.Range(0, houseSetTrack.clickSounds.Length)];
        PlayHouseSetEffect(clip);
    }


    // 播放UI悬停音效
    public void PlayUIHover()
    {
        if (uiTrack.source == null || uiTrack.hoverSounds == null || uiTrack.hoverSounds.Length == 0)
            return;

        AudioClip clip = uiTrack.hoverSounds[Random.Range(0, uiTrack.hoverSounds.Length)];
        PlayUIEffect(clip);
    }
    // 砖石
    public void PlayHouseSetStone()
    {
        if (houseSetTrack.source == null || houseSetTrack.hoverSounds == null || houseSetTrack.hoverSounds.Length == 0)
            return;

        AudioClip clip = houseSetTrack.hoverSounds[Random.Range(0, houseSetTrack.hoverSounds.Length)];
        PlayHouseSetEffect(clip);
    }
    private void PlayUIEffect(AudioClip clip)
    {
        if (clip == null || uiTrack.source == null) return;

        // 设置音量并播放
        uiTrack.source.volume = uiTrack.volume;
        uiTrack.source.PlayOneShot(clip);

        if (showDebugInfo)
            Debug.Log($"[UI音效] 播放: {clip.name} (音量: {uiTrack.volume})");
    }
    void PlayHouseSetEffect(AudioClip clip)
    {
        if (clip == null || houseSetTrack.source == null) return;

        // 设置音量并播放
        houseSetTrack.source.volume = houseSetTrack.volume;
        houseSetTrack.source.PlayOneShot(clip);

        if (showDebugInfo)
            Debug.Log($"[房屋设置音效] 播放: {clip.name} (音量: {houseSetTrack.volume})");
    }


    // 播放自定义UI音效
    public void PlayCustomUIEffect(AudioClip clip)
    {
        if (clip == null || uiTrack.source == null) return;

        // 设置音量并播放
        uiTrack.source.volume = uiTrack.volume;
        uiTrack.source.PlayOneShot(clip);

        if (showDebugInfo)
            Debug.Log($"[UI音效] 播放自定义: {clip.name} (音量: {uiTrack.volume})");
    }

    // 加载保存的音量
    private void LoadSavedVolumes()
    {
        // BGM音量
        if (PlayerPrefs.HasKey("BGM_Volume"))
        {
            float volume = PlayerPrefs.GetFloat("BGM_Volume");
            SetTrackVolume(bgmTrack, volume);
            Debug.Log($"加载保存的BGM音量: {volume}");
        }

        // 环境音量
        if (PlayerPrefs.HasKey("ENV_Volume"))
        {
            float volume = PlayerPrefs.GetFloat("ENV_Volume");
            SetTrackVolume(envTrack, volume);
            Debug.Log($"加载保存的环境音量: {volume}");
        }

        // UI音量
        if (PlayerPrefs.HasKey("UI_Volume"))
        {
            uiTrack.volume = PlayerPrefs.GetFloat("UI_Volume");
            Debug.Log($"加载保存的UI音量: {uiTrack.volume}");
        }
        // 房屋设置音量
        if (PlayerPrefs.HasKey("HouseSet_Volume"))
        {
            houseSetTrack.volume = PlayerPrefs.GetFloat("HouseSet_Volume");
            Debug.Log($"加载保存的房屋设置音量: {houseSetTrack.volume}");
        }
    }

    // 保存音量设置
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("BGM_Volume", bgmTrack.volumeMultiplier);
        PlayerPrefs.SetFloat("ENV_Volume", envTrack.volumeMultiplier);
        PlayerPrefs.SetFloat("UI_Volume", uiTrack.volume);
        PlayerPrefs.SetFloat("HouseSet_Volume", houseSetTrack.volume);
        PlayerPrefs.Save();
    }

    // 调试信息
    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;

        GUI.Label(new Rect(10, 10, 500, 30), $"全局音频系统 | 场景: {currentScene.name}", style);

        DrawTrackInfo(bgmTrack, 40);
        DrawTrackInfo(envTrack, 70);

        // UI音效信息
        if (uiTrack.source != null)
        {
            string uiStatus = uiTrack.source.isPlaying ? "播放中" : "空闲";
            string clipsInfo = $"点击音效: {uiTrack.clickSounds.Length}, 悬停音效: {uiTrack.hoverSounds.Length}";

            GUI.Label(new Rect(10, 100, 500, 30),
                $"UI音效: {uiStatus} | 音量: {uiTrack.volume:F2} | {clipsInfo}",
                new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = Color.cyan } });
        }
    }

    private void DrawTrackInfo(AudioTrack track, float yPos)
    {
        if (track == null) return;

        string status = track.isPlaying ?
            (track.source.isPlaying ? "播放中" : "暂停中") :
            "已停止";

        string clipInfo = track.source.clip != null ?
            $"{track.source.clip.name} ({track.source.time:F1}/{track.source.clip.length:F1}s)" :
            "无片段";

        GUI.Label(new Rect(10, yPos, 500, 30),
            $"{track.trackName}: {status} | 音量: {track.source.volume:F2} | 片段: {clipInfo}",
            new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = track.isPlaying ? Color.green : Color.gray } });
    }
}