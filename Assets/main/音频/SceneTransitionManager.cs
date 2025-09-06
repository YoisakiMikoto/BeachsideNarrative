using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("转场设置")]
    public float transitionDuration = 1.0f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Image fadeOverlay;
    
    private bool isTransitioning = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 确保有遮罩
        if (fadeOverlay == null)
        {
            CreateFadeOverlay();
        }
    }
    
    private void CreateFadeOverlay()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        DontDestroyOnLoad(canvasObj);
        
        GameObject overlayObj = new GameObject("FadeOverlay");
        overlayObj.transform.SetParent(canvasObj.transform);
        fadeOverlay = overlayObj.AddComponent<Image>();
        fadeOverlay.color = Color.black;
        fadeOverlay.rectTransform.anchorMin = Vector2.zero;
        fadeOverlay.rectTransform.anchorMax = Vector2.one;
        fadeOverlay.rectTransform.offsetMin = Vector2.zero;
        fadeOverlay.rectTransform.offsetMax = Vector2.zero;
    }
    
    public void LoadSceneWithFade(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionRoutine(sceneName));
        }
    }
    
    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        
        // 通知音频系统即将切换场景
        if (GlobalAudioSystem.Instance != null)
        {
            GlobalAudioSystem.Instance.pauseOnSceneLoad = true;
        }
        
        // 淡出
        yield return StartCoroutine(FadeRoutine(0f, 1f));
        
        // 加载新场景
        SceneManager.LoadScene(sceneName);
        
        // 等待一帧让新场景初始化
        yield return null;
        
        // 淡入
        yield return StartCoroutine(FadeRoutine(1f, 0f));
        
        isTransitioning = false;
    }
    
    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = fadeCurve.Evaluate(timer / transitionDuration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
            if (fadeOverlay != null)
            {
                Color color = fadeOverlay.color;
                color.a = alpha;
                fadeOverlay.color = color;
            }
            
            yield return null;
        }
    }
}