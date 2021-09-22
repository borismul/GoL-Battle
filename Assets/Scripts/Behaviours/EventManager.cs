using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void OnGridCreatedEvent(Vector2 gridSize);
    public static OnGridCreatedEvent OnGridCreated;

    public delegate void CreateMeshEvent(int[,] gridData);
    public static CreateMeshEvent CreateMesh;

    public delegate void UpdateMeshEvent(int[,] gridData);
    public static UpdateMeshEvent UpdateMesh;
}
