
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

[Serializable]
public class BuildWithTex
{
    public GameObject prefab;
    public Texture2D tex;
}

[System.Serializable]
public class PrefabGroup
{
    public string category;                 // 类别名称
    public List<BuildWithTex> prefabs;        // 该类别下的所有预制体
}

public class ModelPlacer : MonoBehaviour
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [Header("放置区域")]
    public Vector3 placementCenter = new Vector3(0, 0, 0);
    public float placementWidth = 200f;
    public float placementLength = 100f;

    [Header("模型分类配置")]
    public List<PrefabGroup> prefabGroups;

    [Header("地面层")]
    public LayerMask groundLayer;

    [Header("边框显示")]
    public GameObject Border;

    [Header("材质模板")]
    public Material TransparentTemplate;

    [Header("放置音效")]
    public List<AudioClip> placeSoundClips;
    public AudioSource audioSource;


    private Dictionary<string, List<BuildWithTex>> prefabDict = new Dictionary<string, List<BuildWithTex>>();
    private GameObject currentModel;
    private bool isPlacing = false;
    private float rotationSpeed = 90f;
    private Material[] originalMaterials;
    private bool isRed = false;
    private bool isDeleteMode = false;
    private string selectedCategory = "";
    private int selectedIndex = 0;

    public UIController UIC;

    void Start()
    {
        Border?.SetActive(false);

        foreach (var group in prefabGroups)
        {
            prefabDict[group.category] = group.prefabs;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        UIC = FindObjectOfType<UIController>();
    }


    void Update()
    {
        if (isPlacing && currentModel != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                Vector3 targetPosition = hit.point;

                Collider modelCollider = currentModel.GetComponent<Collider>();
                if (modelCollider != null)
                {
                    float modelHeight = modelCollider.bounds.size.y / 2f;

                    Ray downwardRay = new Ray(currentModel.transform.position + Vector3.up * modelHeight, Vector3.down);
                    if (Physics.Raycast(downwardRay, out RaycastHit groundHit, 100f, groundLayer))
                    {
                        targetPosition.y = groundHit.point.y + modelHeight;
                    }
                }

                // 判断是否在矩形区域内
                bool isInPlacementArea =
                    Mathf.Abs(targetPosition.x - placementCenter.x) <= placementWidth / 2f &&
                    Mathf.Abs(targetPosition.z - placementCenter.z) <= placementLength / 2f;

                if (isInPlacementArea)
                {
                    currentModel.transform.position = targetPosition;
                }

                // 旋转
                if (Input.GetKey(KeyCode.R))
                {
                    currentModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                }

                // 检查碰撞
                CollisionCheck check = currentModel.GetComponentInChildren<CollisionCheck>();
                bool hasCollision = check != null && check.isCollision;

                if (!hasCollision && isInPlacementArea)
                {
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        isPlacing = false;

                        Border.SetActive(false);

                        UIC.CompletePlacing();

                        PlayRandomPlaceSound(); 

                        RestoreOriginalMaterials();
                        Border?.SetActive(false);
                        currentModel.GetComponentInChildren<PlacingEffect>()?.StartToPlace();
                        currentModel = null;
                    }

                    if (isRed) SetWhiteMaterials();
                }
                else
                {
                    if (!isRed) SetRedMaterials();
                }
            }
        }


        // 全局删除模式

        if (isDeleteMode && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider != null)
                {
                    string hitTag = hit.collider.tag;

                    if (hitTag == "Placeable" || hitTag == "PathObject")
                    {
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    public void CancelPlacing()
    {
        if (isPlacing)
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
            }

            currentModel = null;
            isPlacing = false;
            isRed = false;
            Border?.SetActive(false);
        }
    }



    public void SelectPrefab(string category, int index)
    {
        selectedCategory = category;
        selectedIndex = index;
    }

    public void SpawnModel()
    {
        CancelPlacing(); 

        if (!prefabDict.ContainsKey(selectedCategory)) return;

        var prefabs = prefabDict[selectedCategory];
        if (selectedIndex < 0 || selectedIndex >= prefabs.Count) return;

        currentModel = Instantiate(prefabs[selectedIndex].prefab);
        currentModel.tag = "Placeable";
        isPlacing = true;
        Border?.SetActive(true);

        ApplyTransparentMaterials(); 
    }

    public void OnClick_Select_House1()
    {
        SelectPrefab("House", 0);
        SpawnModel();
    }

    public void OnClick_Select_House2()
    {
        SelectPrefab("House", 1);
        SpawnModel();
    }

    public void OnClick_Select_House3()
    {
        SelectPrefab("House", 2);
        SpawnModel();
    }

    public void OnClick_Select_House4()
    {
        SelectPrefab("House", 3);
        SpawnModel();
    }

    public void OnClick_Select_House5()
    {
        SelectPrefab("House", 4);
        SpawnModel();
    }

    public void OnClick_Select_Tree1()
    {
        SelectPrefab("Tree", 0);
        SpawnModel();
    }

    public void OnClick_Select_Tree2()
    {
        SelectPrefab("Tree", 1);
        SpawnModel();
    }

    public void OnClick_Select_Tree3()
    {
        SelectPrefab("Tree", 2);
        SpawnModel();
    }

    public void OnClick_Select_Other1()
    {
        SelectPrefab("Other", 0);
        SpawnModel();
    }

    public void OnClick_Select_Other2()
    {
        SelectPrefab("Other", 1);
        SpawnModel();
    }

    public void OnClick_Select_Other3()
    {
        SelectPrefab("Other", 2);
        SpawnModel();
    }

    public void OnClick_Select_Other4()
    {
        SelectPrefab("Other", 3);
        SpawnModel();
    }

    public void OnClick_Select_Other5()
    {
        SelectPrefab("Other", 4);
        SpawnModel();
    }

    public void ToggleDeleteMode()
    {
        isDeleteMode = !isDeleteMode;

        if (isDeleteMode)
        {
            Debug.Log("点击可删除");
        }
        else
        {
            Debug.Log("删除模式已关闭");
        }
    }


    private void ApplyTransparentMaterials()
    {
        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;

            // 使用外部模板材质创建新实例，避免Shader问题
            Material newMat = new Material(TransparentTemplate);
            Color col = newMat.color;
            col.a = 0.5f; // 保险起见再调一次透明度
            newMat.color = col;
            newMat.SetTexture(MainTex,prefabDict[selectedCategory][selectedIndex].tex); //改tex，防止贴图错误

            renderers[i].material = newMat;
        }
    }


    private void SetRedMaterials()
    {
        isRed = true;
        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
        // foreach (var rend in renderers)
        // {
        //     Color color = Color.red;
        //     color.a = rend.material.color.a;
        //     rend.material.color = color;
        // }
        
        Color color = Color.red;
        color.a = renderers[0].material.color.a;
        renderers[0].material.color = color;
    }

    private void SetWhiteMaterials()
    {
        isRed = false;
        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
        // foreach (var rend in renderers)
        // {
        //     Color color = Color.white;
        //     color.a = rend.material.color.a;
        //     rend.material.color = color;
        // }
        Color color = Color.white;
        color.a = renderers[0].material.color.a;
        renderers[0].material.color = color;
    }

    private void RestoreOriginalMaterials()
    {
        if (currentModel == null) return;
        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length && i < originalMaterials.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }
    }

    private void PlayRandomPlaceSound()
    {
        if (placeSoundClips == null || placeSoundClips.Count == 0 || audioSource == null)
            return;

        AudioClip clip = placeSoundClips[UnityEngine.Random.Range(0, placeSoundClips.Count)];
        audioSource.PlayOneShot(clip);
    }


    private Material CreateTransparentMaterial(Material source)
    {
        Material mat = new Material(source);

        // 设置渲染模式为 Transparent
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // 设置颜色 alpha
        Color col = mat.color;
        col.a = 0.5f;
        mat.color = col;

        return mat;
    }

}
