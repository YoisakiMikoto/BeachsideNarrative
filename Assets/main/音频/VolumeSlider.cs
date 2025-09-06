using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { BGM, Environment }

    public VolumeType volumeType = VolumeType.BGM;
    
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        
        // 确保AudioManager已存在
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager实例不存在！");
            return;
        }
        
        // 设置初始值
        switch (volumeType)
        {
            case VolumeType.BGM:
                slider.value = AudioManager.Instance.BGMVolume;
                break;
            case VolumeType.Environment:
                slider.value = AudioManager.Instance.EnvVolume;
                break;
        }
        
        Debug.Log($"滑块初始化: {volumeType}, 值={slider.value}");

        // 添加事件监听
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    void OnDestroy()
    {
        // 安全移除监听器
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    /// <summary>
    /// 当滑动条值改变时调用的方法，根据音量类型设置对应的音量
    /// </summary>
    /// <param name="value">当前滑动条的值，范围通常为0-1</param>
    /// <remarks>
    /// 该方法会检查AudioManager实例是否存在，并根据volumeType设置BGM或环境音量
    /// 最后会调用LogMixerStatus方法输出当前混音器状态用于调试
    /// </remarks>
    private void OnSliderValueChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        
        switch (volumeType)
        {
            case VolumeType.BGM:
                AudioManager.Instance.SetBGMVolume(value);
                break;
            case VolumeType.Environment:
                AudioManager.Instance.SetEnvVolume(value);
                break;
        }
    }
}