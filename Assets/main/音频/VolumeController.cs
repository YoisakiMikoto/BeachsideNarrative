using UnityEngine;
using UnityEngine.UI;

public enum VolumeType
{
    BGM,
    Environment,
    UI
}

public class VolumeController : MonoBehaviour
{
    [Header("音量类型")]
    public VolumeType volumeType;
    
    [Header("UI元素")]
    public Slider volumeSlider;
    public Text volumeLabel;
    public Image soundIcon; // 可选：静音图标
    
    [Header("图标 (可选)")]
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;
    
    [Header("设置")]
    [Range(0f, 1f)] public float defaultVolume = 0.8f;
    public bool showMuteIcon = false; // 是否显示静音图标

    void Start()
    {
        // 确保全局音频系统存在
        if (GlobalAudioSystem.Instance == null)
        {
            Debug.LogError("全局音频系统未初始化!");
            return;
        }

        // 初始化滑块
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            
            // 设置初始值（只读取不改变音量）
            volumeSlider.value = GetCurrentVolume();
            
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        UpdateUI();
    }

    // 获取当前音量值
    private float GetCurrentVolume()
    {
        if (GlobalAudioSystem.Instance == null) return defaultVolume;
        
        switch (volumeType)
        {
            case VolumeType.BGM:
                return GlobalAudioSystem.Instance.bgmTrack.volumeMultiplier;
            case VolumeType.Environment:
                return GlobalAudioSystem.Instance.envTrack.volumeMultiplier;
            case VolumeType.UI:
                return GlobalAudioSystem.Instance.uiTrack.volume;
            default:
                return defaultVolume;
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (GlobalAudioSystem.Instance == null) return;
        
        switch (volumeType)
        {
            case VolumeType.BGM:
                GlobalAudioSystem.Instance.SetTrackVolume(
                    GlobalAudioSystem.Instance.bgmTrack, value);
                break;
            case VolumeType.Environment:
                GlobalAudioSystem.Instance.SetTrackVolume(
                    GlobalAudioSystem.Instance.envTrack, value);
                    GlobalAudioSystem.Instance.SetHouseSetVolume(value);
                break;
            case VolumeType.UI:
                GlobalAudioSystem.Instance.SetUIVolume(value);
                break;
        }
        
        // 更新UI显示
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 更新标签
        if (volumeLabel != null && volumeSlider != null)
        {
            volumeLabel.text = $"{Mathf.RoundToInt(volumeSlider.value * 100)}%";
        }
        
        // 更新图标（如果启用）
        if (showMuteIcon && soundIcon != null && soundOnIcon != null && soundOffIcon != null)
        {
            soundIcon.sprite = volumeSlider.value > 0.01f ? soundOnIcon : soundOffIcon;
        }
    }
    
    // 切换静音
    public void ToggleMute()
    {
        if (volumeSlider == null) return;
        
        if (volumeSlider.value > 0.01f)
        {
            // 保存当前音量后静音
            PlayerPrefs.SetFloat($"{volumeType}_PreMute", volumeSlider.value);
            volumeSlider.value = 0f;
        }
        else
        {
            // 恢复静音前的音量
            string prefKey = $"{volumeType}_PreMute";
            if (PlayerPrefs.HasKey(prefKey))
            {
                volumeSlider.value = PlayerPrefs.GetFloat(prefKey);
            }
            else
            {
                volumeSlider.value = defaultVolume;
            }
        }
        
        UpdateUI();
    }
    
    // 设置音量而不触发事件
    public void SetVolumeWithoutNotify(float volume)
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
            UpdateUI();
        }
    }
}
