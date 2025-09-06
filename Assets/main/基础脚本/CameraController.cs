using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度
    public float rotationSpeed = 100f; // 旋转速度
    public float zoomSpeed = 10f; // 缩放速度
    public float smoothTime = 0.2f; // 平滑时间
    public float minHeight = 5f; // 缩放最小高度
    public float maxHeight = 20f; // 缩放最大高度
    public float bounceFactor = 0.2f; // 反弹系数
    public float minX,minZ,maxX,maxZ; // 限制相机移动范围

    public float maxMoveSpeed = 20f;
    public float edgeSize = 100f;
    public float acceleration = 5f;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 velocity = Vector3.zero;
    private Vector3 vel;
    private float rotationVelocity;
    private float zoomVelocity;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = Quaternion.Euler(30f, transform.eulerAngles.y, 0f);
    }

    void Update()
    {
        HandleMouseMove();
        HandleMovement();
        HandleRotation();
        HandleZoom();
        SmoothTransition();
    }

    void HandleMouseMove()
    {
        Vector2 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        Vector3 moveDir = Vector3.zero;

        if (mousePos.x > 0 && mousePos.x < screenWidth && mousePos.y > 0 && mousePos.y < screenHeight)
        {
            if (mousePos.x > screenWidth - edgeSize)
            {
                float t = (mousePos.x - (screenWidth - edgeSize)) / edgeSize;
                moveDir += moveSpeed * Time.deltaTime * t * right;
                float offy = (mousePos.y - screenHeight / 2) / (screenHeight / 2);
                moveDir += moveSpeed * Time.deltaTime * t * offy * forward;
            }
            else if (mousePos.x < edgeSize)
            {
                float t = 1f - (mousePos.x / edgeSize);
                moveDir -= moveSpeed * Time.deltaTime * t * right;
                float offy = (mousePos.y - screenHeight / 2) / (screenHeight / 2);
                moveDir += moveSpeed * Time.deltaTime * t * offy * forward;
            }
            if (mousePos.y > screenHeight - edgeSize)
            {
                float t = (mousePos.y - (screenHeight - edgeSize)) / edgeSize;
                moveDir += moveSpeed * Time.deltaTime * t * forward;
                float offx = (mousePos.x - screenWidth / 2) / (screenWidth / 2);
                moveDir += moveSpeed * Time.deltaTime * t * offx * right;
            }
            else if (mousePos.y < edgeSize)
            {
                float t = 1f - (mousePos.y / edgeSize);
                moveDir -= moveSpeed * Time.deltaTime * t * forward;
                float offx = (mousePos.x - screenWidth / 2) / (screenWidth / 2);
                moveDir += moveSpeed * Time.deltaTime * t * offx * right;
            }
        }
        targetPosition += moveDir;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        targetPosition += (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
            targetRotation = Quaternion.Euler(30f, transform.eulerAngles.y - rotationSpeed * Time.deltaTime, 0f);
        
        if (Input.GetKey(KeyCode.E))
            targetRotation = Quaternion.Euler(30f, transform.eulerAngles.y + rotationSpeed * Time.deltaTime, 0f);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // 计算摄像机的移动方向
            Vector3 zoomDirection = transform.forward.normalized;
            
            // 计算新的目标位置
            Vector3 newPosition = targetPosition + zoomDirection * scroll * zoomSpeed;
            
            // 计算新位置的高度
            float newY = newPosition.y;
            
            // 如果超出高度限制，应用回弹效果
            if (newY < minHeight)
            {
                float overshoot = minHeight - newY;
                newPosition = targetPosition + zoomDirection * (-overshoot * bounceFactor);
                newPosition.y = minHeight;
            }
            else if (newY > maxHeight)
            {
                float overshoot = newY - maxHeight;
                newPosition = targetPosition - zoomDirection * (overshoot * bounceFactor);
                newPosition.y = maxHeight;
            }
            
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
            targetPosition = newPosition;
        }
    }

    void SmoothTransition()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 0.1f);
    }
}
