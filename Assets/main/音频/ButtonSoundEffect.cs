using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("音效设置")]
    public bool enableClickSound = true;
    public bool enableHoverSound = true;
    
    [Header("自定义音效 (可选)")]
    public AudioClip customClickSound;
    public AudioClip customHoverSound;
    
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        
        // 如果全局系统不存在，禁用组件
        if (GlobalAudioSystem.Instance == null)
        {
            Debug.LogWarning("全局音频系统未找到，按钮音效已禁用");
            enabled = false;
        }
    }
    
    // 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enableClickSound || !button.interactable) return;
        
        if (customClickSound != null)
        {
            // 播放自定义点击音效
            GlobalAudioSystem.Instance.PlayCustomUIEffect(customClickSound);
        }
        else
        {
            // 播放默认点击音效
            GlobalAudioSystem.Instance.PlayUIClick();
        }
    }
    
    // 悬停事件
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHoverSound || !button.interactable) return;
        
        if (customHoverSound != null)
        {
            // 播放自定义悬停音效
            GlobalAudioSystem.Instance.PlayCustomUIEffect(customHoverSound);
        }
        else
        {
            // 播放默认悬停音效
            GlobalAudioSystem.Instance.PlayUIHover();
        }
    }
    
    // 手动触发点击音效（用于代码调用）
    public void TriggerClickSound()
    {
        OnPointerClick(null);
    }
    
    // 手动触发悬停音效（用于代码调用）
    public void TriggerHoverSound()
    {
        OnPointerEnter(null);
    }
}
