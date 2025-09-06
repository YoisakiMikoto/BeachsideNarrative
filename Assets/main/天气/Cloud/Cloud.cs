using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Cloud : MonoBehaviour
{
    // ========== 生成参数 ========== //
    [Header("基础设置"), Tooltip("云朵预制体引用")]
    public GameObject cloudPrefab;
    
    [Tooltip("场景中允许的最大云朵数量"), Min(1)]
    public int maxClouds = 20;
    
    [Tooltip("生成间隔（秒）"), Min(0.1f)]
    public float spawnInterval = 2f;

    // ========== 运动参数 ========== //
    [Header("移动设置"), Tooltip("移动方向向量（建议归一化）")]
    public Vector3 moveDirection = Vector3.right;
    
    [Tooltip("基础移动速度"), Min(0)]
    public float baseSpeed = 1f;
    
    [Tooltip("速度随机变化幅度"), Range(0, 1)]
    public float speedVariation = 0.2f;

    // ========== 生成范围 ========== //
    [Header("生成范围"), Tooltip("水平生成范围（X轴）")]
    public Vector2 horizontalRangeX = new(-15, 15);
    
    [Tooltip("水平生成范围（Z轴）")]
    public Vector2 horizontalRangeZ = new(-15, 15);
    
    [Tooltip("垂直生成范围（Y轴）")]
    public Vector2 verticalRange = new(-5, 5);
    
    [Tooltip("尺寸随机范围")]
    public Vector2 sizeRange = new(0.8f, 1.2f);
    
    // 新增初始生成参数
    [Header("初始设置")]
    [Tooltip("游戏开始时立即生成的云朵数量")]
    [SerializeField, Min(0)] private int initialClouds = 10;
    [Header("初始生成范围"), Tooltip("水平生成范围（X轴）")]
    public Vector2 initialRangeX = new(-15, 15);
    
    [Tooltip("水平生成范围（Z轴）")]
    public Vector2 initialRangeZ = new(-15, 15);

    // ========== 回收设置 ========== //
    [Header("回收设置"), Tooltip("消失动画后的缓冲时间")]
    public float disappearDelay = 0.5f;
    
    [Header("回收范围"), Tooltip("水平回收范围（X轴）")]
    public Vector2 disappearRangeX = new(-15, 15);
    
    [Tooltip("水平回收范围（Z轴）")]
    public Vector2 disappearRangeZ = new(-15, 15);


    private float spawnRate =1f;
    // ========== 内部状态 ========== //
    private IObjectPool<GameObject> _cloudPool;          // 对象池实例
    private List<CloudEntity> _activeClouds = new();     // 活动云列表
    private bool _isBoomSpawn; // 初始生成状态标识
    /// <summary>
    /// 初始化对象池和生成器
    /// </summary>
    void Start()
    {
        InitializePool();
        PrewarmClouds(initialClouds);    // 预生成方法
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 每帧更新云朵状态
    /// </summary>
    void Update()
    {
        UpdateCloudsMovement();
        HandleOutOfBounds();
    }

    /// <summary>
    /// 初始化对象池配置
    /// </summary>
    private void InitializePool()
    {
        _cloudPool = new ObjectPool<GameObject>(
            // 对象创建方法
            createFunc: () => {
                GameObject newCloud = Instantiate(cloudPrefab);
                var entity = newCloud.GetComponent<CloudEntity>();
                entity.pool = _cloudPool; // 注入池引用
                return newCloud;
            },
            
            // 从池中取出时的处理
            actionOnGet: cloud => {
                cloud.SetActive(true);
                CloudEntity entity = cloud.GetComponent<CloudEntity>();
                
                // 初始化参数
                entity.Initialize(
                    poolRef: _cloudPool,
                    speed: GetRandomSpeed()
                );
                
                // 设置初始属性
                cloud.transform.SetPositionAndRotation(
                    GetRandomPosition(_isBoomSpawn), 
                    Quaternion.identity
                );
                cloud.transform.localScale = GetRandomScale();
                
                // 加入活动列表
                _activeClouds.Add(entity);
            },
            
            // 放回池中时的处理
            actionOnRelease: cloud => {
                CloudEntity entity = cloud.GetComponent<CloudEntity>();
                _activeClouds.Remove(entity); // 从活动列表移除
                cloud.SetActive(false);
            },
            
            // 对象销毁方法
            actionOnDestroy: Destroy,
            
            // 关闭集合检查提升性能
            collectionCheck: false,
            
            // 容量设置
            defaultCapacity: 10,
            maxSize: maxClouds * 2
        );
    }

    /// <summary>
    /// 定时生成协程
    /// </summary>
    private System.Collections.IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 数量未达上限时生成
            if (_activeClouds.Count < maxClouds)
                _cloudPool.Get();
            
            yield return new WaitForSeconds(spawnInterval*spawnRate);
        }
    }

    /// <summary>
    /// 更新所有云朵的位置
    /// </summary>
    private void UpdateCloudsMovement()
    {
        // 遍历所有活动云
        foreach (CloudEntity entity in _activeClouds)
        {
            entity.UpdateMovement(moveDirection.normalized);
        }
    }

    /// <summary>
    /// 处理越界回收
    /// </summary>
    private void HandleOutOfBounds()
    {
        // 临时列表避免迭代时修改集合
        List<CloudEntity> toDisappear = new();
        
        // 检测所有活动云
        foreach (CloudEntity entity in _activeClouds)
        {
            if (!entity.IsDisappearing && IsOutOfView(entity.transform.position))
            {
                toDisappear.Add(entity);
            }
        }
        
        // 执行回收
        foreach (CloudEntity entity in toDisappear)
            entity.StartDisappear(disappearDelay);
    }

    /// <summary>
    /// 判断是否超出范围
    /// </summary>
    private bool IsOutOfView(Vector3 worldPos)
    {
        //看是否超出范围
        if (worldPos.x <= disappearRangeX.y && worldPos.x >= disappearRangeX.x
            &&worldPos.z <= disappearRangeZ.y && worldPos.z >= disappearRangeZ.x)
        {
            return false;
        }
        
        return true;
    }

    // ========== 工具方法 ========== //
    
    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    private Vector3 GetRandomPosition(bool isBoom)
    {
        return isBoom?
            new Vector3(
                Random.Range(initialRangeX.x, initialRangeX.y),
                Random.Range(verticalRange.x, verticalRange.y),
                Random.Range(initialRangeZ.x, initialRangeZ.y))
                :
            new Vector3(
            Random.Range(horizontalRangeX.x, horizontalRangeX.y),
            Random.Range(verticalRange.x, verticalRange.y),
            Random.Range(horizontalRangeZ.x, horizontalRangeZ.y));
    }

    /// <summary>
    /// 获取随机缩放值
    /// </summary>
    private Vector3 GetRandomScale()
    {
        return Vector3.one * Random.Range(sizeRange.x, sizeRange.y);
    }

    /// <summary>
    /// 计算带随机变化的速度
    /// </summary>
    private float GetRandomSpeed()
    {
        return baseSpeed * (1 + Random.Range(-speedVariation, speedVariation));
    }
    
    /// <summary>
    /// 预生成大量云朵
    /// </summary>
    public void PrewarmClouds(int nums)
    {
        _isBoomSpawn = true;
        // 计算实际要生成的云朵数量（不能超过最大数量）
        int spawnAmount = Mathf.Clamp(nums, 0, maxClouds);
        
        // 分帧生成避免卡顿（每帧生成5个）
        StartCoroutine(PrewarmCoroutine(spawnAmount));
    }

    /// <summary>
    /// 分帧预生成协程
    /// </summary>
    private IEnumerator PrewarmCoroutine(int total)
    {
        int count = 0;
        const int perFrame = 5; // 每帧生成数量
        
        while (count < total)
        {
            int batch = Mathf.Min(perFrame, total - count);
            for (int i = 0; i < batch; i++)
            {
                _cloudPool.Get();
                count++;
            }
            yield return null; // 下一帧继续
        }

        _isBoomSpawn = false;
    }

    public void SetSpawnRate(float rate)
    {
        spawnRate = 1/rate;
    }

    // ========== 调试辅助 ========== //
    void OnDrawGizmosSelected()
    {
        // 在场景视图中绘制生成区域
        Gizmos.color = new Color(0,1,0,0.5f);
        Vector3 center1 = new Vector3(
            (horizontalRangeX.x + horizontalRangeX.y) / 2,
            (verticalRange.x + verticalRange.y) / 2,
            (horizontalRangeZ.x + horizontalRangeZ.y) / 2
        );
        Vector3 size1 = new Vector3(
            horizontalRangeX.y - horizontalRangeX.x,
            verticalRange.y - verticalRange.x,
            horizontalRangeZ.y - horizontalRangeZ.x
        );
        Gizmos.DrawCube(center1, size1);
        // 在场景视图中绘制消失区域
        Gizmos.color = new Color(1,0,0,0.5f);
        Vector3 center2 = new Vector3(
            (disappearRangeX.x + disappearRangeX.y) / 2,
            (verticalRange.x + verticalRange.y) / 2,
            (disappearRangeZ.x + disappearRangeZ.y) / 2
        );
        Vector3 size2 = new Vector3(
            disappearRangeX.y - disappearRangeX.x,
            0f,
            disappearRangeZ.y - disappearRangeZ.x
        );
        Gizmos.DrawCube(center2, size2);
        // 在场景视图中绘制预生成区域
        Gizmos.color = new Color(0,0,1,0.5f);
        Vector3 center3 = new Vector3(
            (initialRangeX.x + initialRangeX.y) / 2,
            (verticalRange.x + verticalRange.y) / 2,
            (initialRangeZ.x + initialRangeZ.y) / 2
        );
        Vector3 size3 = new Vector3(
            initialRangeX.y - initialRangeX.x,
            verticalRange.y - verticalRange.x,
            initialRangeZ.y - initialRangeZ.x
        );
        Gizmos.DrawCube(center3, size3);
    }
}
