using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tern : MonoBehaviour
{
    Material mat;
    
    public float time = 3f;
     [SerializeField]
    AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.25f),new Keyframe(1, 1));
    
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetFloat("_TransValue", 1);

        StartCoroutine(MyCoroutine());
    }

    /// <summary>
    /// 协程方法，演示基本的延时功能。
    /// 该方法会在开始时打印日志，等待2秒后再次打印日志。
    /// </summary>
    /// <returns>返回一个IEnumerator用于Unity协程系统</returns>
    IEnumerator MyCoroutine()
    {
        //Debug.Log("Coroutine started");
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float rate = curve.Evaluate(elapsedTime / time);
            rate = Mathf.Clamp01(rate);
            mat.SetFloat("_TransValue", 1-rate); // Assuming the material has a property named "_Rate"
            yield return null;
        }
        mat.SetFloat("_TransValue", 0);
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
