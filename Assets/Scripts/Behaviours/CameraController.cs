using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    Camera cameraController;

    private void Awake()
    {
        cameraController = GetComponent<Camera>();
        EventManager.OnGridCreated += UpdatePosition;        
    }

    void UpdatePosition(Vector2 gridSize)
    {
        cameraController.orthographicSize = Mathf.Max(gridSize.x, gridSize.y)/2;
    }
}
