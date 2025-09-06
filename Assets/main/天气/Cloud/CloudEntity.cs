using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class CloudEntity : MonoBehaviour
{
    // ========== 公开字段 ========== //
    [Tooltip("出现动画在Animator中的状态名称")]
    public string appearAnimation = "Appear";
    [Tooltip("消失动画在Animator中的状态名称")]
    public string disappearAnimation = "Disappear";
    
    // ========== 对象池相关 ========== //
    [System.NonSerialized] public IObjectPool<GameObject> pool; // 所属对象池引用（不序列化）
    
    // ========== 组件引用 ========== //
    private Animator _animator;
    private Transform _transform;
    
    // ========== 运动参数 ========== //
    private float _movementSpeed;    // 当前移动速度
    public bool IsDisappearing { get; private set; } // 消失状态标识

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    void Awake()
    {
        // 获取必要组件
        _transform = transform;
        _animator = GetComponent<Animator>();
        
        // 安全性检查
        if (_animator == null)
            Debug.LogError("云朵预制体必须包含Animator组件", gameObject);
    }

    /// <summary>
    /// 初始化云朵状态（由CloudManager调用）
    /// </summary>
    /// <param name="poolRef">所属对象池</param>
    /// <param name="speed">移动速度</param>
    public void Initialize(IObjectPool<GameObject> poolRef, float speed)
    {
        pool = poolRef;
        _movementSpeed = speed;
        PlayAppearAnimation();
    }

    /// <summary>
    /// 播放出现动画并重置状态
    /// </summary>
    private void PlayAppearAnimation()
    {
        IsDisappearing = false;
        _animator.Play(appearAnimation); // 播放指定动画
    }

    /// <summary>
    /// 每帧移动更新（由CloudManager驱动）
    /// </summary>
    /// <param name="direction">标准化移动方向</param>
    public void UpdateMovement(Vector3 direction)
    {
        // 基于世界坐标系的匀速移动
        _transform.Translate(direction * (_movementSpeed * Time.deltaTime), Space.World);
    }

    /// <summary>
    /// 启动消失流程（外部触发）
    /// </summary>
    /// <param name="delay">额外缓冲时间</param>
    public void StartDisappear(float delay)
    {
        if (!IsDisappearing)
            StartCoroutine(DisappearProcess(delay));
    }

    /// <summary>
    /// 消失动画处理协程
    /// </summary>
    private System.Collections.IEnumerator DisappearProcess(float delay)
    {
        IsDisappearing = true; // 设置状态标识
        
        // 播放消失动画
        _animator.Play(disappearAnimation);
        
        // 计算总等待时间（动画长度+缓冲时间）
        float totalWaitTime = GetAnimationLength(disappearAnimation) + delay;
        yield return new WaitForSeconds(totalWaitTime);
        
        // 回收到对象池
        if (pool != null)
            pool.Release(gameObject);
    }

    /// <summary>
    /// 获取指定动画片段的时长
    /// </summary>
    private float GetAnimationLength(string clipName)
    {
        // 遍历所有动画片段查找匹配项
        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            if (clip.name.Equals(clipName)) 
                return clip.length;
        
        Debug.LogWarning($"未找到动画片段: {clipName}");
        return 0;
    }
}