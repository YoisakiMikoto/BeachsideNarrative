using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoneRandomRotation : MonoBehaviour
{
    private void Start()
    {
        foreach (Transform child in transform)
        {
            float randomY = Random.Range(0f, 360f);
            child.localRotation = Quaternion.Euler(0f, randomY, 0f);
        }
    }
}
