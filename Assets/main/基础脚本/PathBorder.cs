using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathBorder : MonoBehaviour
{
    [SerializeField] private float _xValue = 10f;
    [SerializeField] private float _yValue = 5f;
    [SerializeField] private float _zValue = 10f;
    private Bounds _bounds;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(_xValue, _yValue, _zValue));
    }

    private void Update()
    {
        _bounds = new Bounds(transform.position, new Vector3(_xValue, _yValue, _zValue));
    }

    public bool IsInsideBorder(Vector3 point)
    {
        return _bounds.Contains(point);
    }
}
