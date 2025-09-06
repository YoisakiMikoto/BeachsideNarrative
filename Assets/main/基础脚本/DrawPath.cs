using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DrawPath : MonoBehaviour
{
    public enum State
    {
        Disabled,
        Drawing,
        Deleting
    }
    [SerializeField]
    private State _state = State.Disabled;
    
    [HideInInspector]
    public bool drawEnabled = false;
    private PathBorder _border;
    private Camera _mainCamera;
    //private Stack<GameObject> _pathObjectStack = new Stack<GameObject>(50);
    
    [SerializeField] private GameObject _pathPrefab;
    //射线检测参数
    [SerializeField] private LayerMask _canSpawnLayer;
    //生成石头的世界空间间距（unit）
    [SerializeField] private float _spawnWorldDistance = 4f;
    private float _currentDistanceDelta;
    private Vector3 _lastHitPoint;
    //进行射线检测的屏幕空间间距（像素）
    [SerializeField] private float _rayCastScreenDistance = 100f;     //拖拽光标时 间隔多远进行一次ScreenPointToRay检测
    private float _currentScreenDelta = 0f;
    private Vector3 _lastMousePos;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        _border = FindObjectOfType<PathBorder>();
    }
    
    private void Update()
    {
        switch (_state)
        {
            case State.Disabled:
                break;
            case State.Drawing:
                DragMouseToDraw();
                break;
            case State.Deleting:
                DeleteSelectedPathObj();
                break;
        }
    }
    
    private void DragMouseToDraw()
    {
        if (Input.GetMouseButton(0))
        {
            _currentScreenDelta += Vector3.Distance(Input.mousePosition, _lastMousePos);

            // 鼠标拖拽经过一定ScreenSpace像素距离后，再进行检测
            if (_currentScreenDelta > _rayCastScreenDistance)
            {
                SpawnAtHitPoint();
                _currentScreenDelta = 0f;
            }
            
            _lastMousePos = Input.mousePosition;
        }
        //松开鼠标  重置数据
        else
        {
            _currentScreenDelta = 0f;
            _currentScreenDelta = 0f;
        }
    }

    // private void DeletePathObj()
    // {
    //     if (_pathObjectStack.Count <= 0)
    //     {
    //         return;
    //     }
    //     
    //     GameObject lastPathObj = _pathObjectStack.Pop();
    //     Destroy(lastPathObj);
    // }

    private void DeleteSelectedPathObj()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }
        
        
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("PathObject"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }

    public void SetState(State state)
    {
        _state = state; 
    }

    private void SpawnAtHitPoint()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //检查是否在边界范围内
            if (!_border.IsInsideBorder(hit.point))
            {
                Debug.Log("not inside border");
                return;
            }
            
            //检查rayCast击中物体的layer是否是可放置路面的layer
            if (((1 << hit.collider.gameObject.layer) & _canSpawnLayer.value) == 0)
            {
                Debug.Log("cannot spawn on this layer");
                return;
            }

            GameObject hitObject = hit.collider.gameObject;
            Vector3 hitPoint = hit.point;

            _currentDistanceDelta += Vector3.Distance(hitPoint, _lastHitPoint);
            _lastHitPoint = hitPoint;
            //未到生成间距 直接返回
            if (_currentDistanceDelta < _spawnWorldDistance)
            {
                return;
            }

            //到生成间距
            _currentDistanceDelta = 0f;
            if (hitObject)
            {
                GameObject pathObj = Instantiate(_pathPrefab, hitPoint, Quaternion.identity);
                //_pathObjectStack.Push(pathObj);
            }
        }
    }
}
