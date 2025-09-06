using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public float shakeIntensity = 0.1f;
    public float shakeFrequency = 1.0f;
    private Vector3 initialPosition;
    private float seedX;
    private float seedY;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
    }

    // Update is called once per frame
    void Update()
    {
        float offsetX = (Mathf.PerlinNoise(seedX, Time.time * shakeFrequency) - 0.5f) * 2f * shakeIntensity;
        float offsetY = (Mathf.PerlinNoise(seedY, Time.time * shakeFrequency) - 0.5f) * 2f * shakeIntensity;
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        Vector3 offset = (right * offsetX) + (up * offsetY);
        transform.position = initialPosition + offset;
    }
}
