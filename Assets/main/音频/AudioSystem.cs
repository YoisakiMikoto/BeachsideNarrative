using UnityEngine;
using System.Collections;

public class AudioSystem : MonoBehaviour
{
    [System.Serializable]
    public class AudioTrackConfig
    {
        public AudioClip[] clips;
        [Space]
        public float fadeInTime = 2f;
        public float fadeOutTime = 3f;
        public float minInterval = 10f;
        public float maxInterval = 15f;
    }

    [Header("Tracks Configuration")]
    public AudioTrackConfig bgmConfig;
    public AudioTrackConfig envConfig;
    
    // 当前播放状态
    private Coroutine bgmCoroutine;
    private Coroutine envCoroutine;
    private bool isBgmFading = false;
    private bool isEnvFading = false;

    void Start()
    {
        Debug.Log("[AudioSystem] 系统启动");
        
        // 确保有AudioManager实例
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[AudioSystem] AudioManager未初始化!");
            return;
        }
        
        // 获取全局音频源
        AudioSource bgmSource = AudioManager.Instance.GetBGMSource();
        AudioSource envSource = AudioManager.Instance.GetEnvSource();
        
        // 检查音频源
        if (bgmSource == null) Debug.LogError("[AudioSystem] BGM音频源为空!");
        if (envSource == null) Debug.LogError("[AudioSystem] 环境音频源为空!");
        
        // 检查音频片段
        if (bgmConfig.clips == null || bgmConfig.clips.Length == 0)
            Debug.LogError("[AudioSystem] 没有BGM音频片段!");
        else
            Debug.Log($"[AudioSystem] 加载了 {bgmConfig.clips.Length} 个BGM片段");
        
        if (envConfig.clips == null || envConfig.clips.Length == 0)
            Debug.LogError("[AudioSystem] 没有环境音频片段!");
        else
            Debug.Log($"[AudioSystem] 加载了 {envConfig.clips.Length} 个环境片段");
        
        // 开始播放循环
        StartAudioTracks(bgmSource, envSource);
    }
    
    void OnDestroy()
    {
        Debug.Log("[AudioSystem] 系统销毁");
        
        // 停止所有协程
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);
        if (envCoroutine != null) StopCoroutine(envCoroutine);
    }

    private void StartAudioTracks(AudioSource bgmSource, AudioSource envSource)
    {
        // 开始BGM循环
        if (bgmConfig.clips != null && bgmConfig.clips.Length > 0)
        {
            Debug.Log("[AudioSystem] 启动BGM轨道");
            bgmCoroutine = StartCoroutine(PlayTrack(bgmSource, bgmConfig, "BGM"));
        }
        
        // 开始环境音循环
        if (envConfig.clips != null && envConfig.clips.Length > 0)
        {
            Debug.Log("[AudioSystem] 启动环境轨道");
            envCoroutine = StartCoroutine(PlayTrack(envSource, envConfig, "Environment"));
        }
    }

    private IEnumerator PlayTrack(AudioSource source, AudioTrackConfig config, string trackName)
    {
        // 确保音频源存在
        if (source == null)
        {
            Debug.LogError($"[{trackName}] 音频源为空!");
            yield break;
        }
        
        // 第一次播放前等待
        float initialDelay = Random.Range(0.5f, 2f);
        Debug.Log($"[{trackName}] 初始延迟: {initialDelay}秒");
        yield return new WaitForSeconds(initialDelay);
        
        while (true)
        {
            // 随机选择音频
            AudioClip clip = config.clips[Random.Range(0, config.clips.Length)];
            
            if (clip == null)
            {
                Debug.LogError($"[{trackName}] 音频片段为空!");
                yield break;
            }
            
            Debug.Log($"[{trackName}] 开始播放: {clip.name} (长度: {clip.length}秒)");
            
            source.clip = clip;
            source.volume = 0f; // 从静音开始
            
            // 播放音频
            source.Play();
            
            // 检查播放状态
            if (!source.isPlaying)
            {
                Debug.LogError($"[{trackName}] 播放失败! 可能原因: " +
                               "音频源未启用 | 音频片段未加载 | 混音器问题");
            }
            
            // 淡入播放
            isBgmFading = true;
            yield return FadeAudio(source, 0f, 1f, config.fadeInTime, trackName);
            isBgmFading = false;
            
            // 等待音频播放（减去淡出时间）
            float playTime = clip.length - config.fadeInTime - config.fadeOutTime;
            
            if (playTime > 0)
            {
                Debug.Log($"[{trackName}] 主播放阶段: {playTime}秒");
                yield return new WaitForSeconds(playTime);
            }
            else
            {
                Debug.LogWarning($"[{trackName}] 播放时间不足! " +
                                $"片段长度: {clip.length}s, 淡入: {config.fadeInTime}s, 淡出: {config.fadeOutTime}s");
            }
            
            // 淡出
            isBgmFading = true;
            yield return FadeAudio(source, 1f, 0f, config.fadeOutTime, trackName);
            isBgmFading = false;
            
            // 随机间隔
            float interval = Random.Range(config.minInterval, config.maxInterval);
            Debug.Log($"[{trackName}] 等待间隔: {interval}秒");
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator FadeAudio(AudioSource source, float startVol, float endVol, float duration, string trackName)
    {
        if (source == null) yield break;
        
        Debug.Log($"[{trackName}] 淡入淡出: {startVol} -> {endVol} ({duration}秒)");
        
        float timer = 0f;
        while (timer < duration)
        {
            if (source == null) yield break;
            
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            source.volume = Mathf.Lerp(startVol, endVol, t);
            
            // 实时显示音量变化
            Debug.Log($"[{trackName}] 音量: {source.volume:F2} | 时间: {timer:F1}/{duration:F1}");
            
            yield return null;
        }
        
        if (source != null)
        {
            source.volume = endVol;
            Debug.Log($"[{trackName}] 淡入淡出完成: 最终音量 {endVol}");
        }
    }
    
    // 添加此方法用于编辑器调试
    public void ForcePlayTest()
    {
        AudioSource testSource = AudioManager.Instance.GetBGMSource();
        if (testSource != null && bgmConfig.clips.Length > 0)
        {
            testSource.volume = 1f;
            testSource.PlayOneShot(bgmConfig.clips[0]);
            Debug.Log("强制播放测试: " + bgmConfig.clips[0].name);
        }
    }
}