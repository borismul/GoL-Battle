using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class MeshController : MonoBehaviour
{
    [SerializeField]
    int cellSize = 1;

    [SerializeField]
    Texture2D initialMapTexture;

    Color[] meshColors;

    MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        EventManager.CreateMesh += BuildMesh;
        EventManager.UpdateMesh += UpdateMesh;
    }

    private void Start()
    {
    }

    // Initialise grid cell from map
    void BuildMesh(int[,] gridMap)
    {
        // Create grid cells data.
        GridCellData[,] meshData = CreateMeshData(gridMap);

        // Create the final cell mesh.
        CreateMesh(meshData);

        // Call OnGridCreated event.
        EventManager.OnGridCreated?.Invoke(new Vector2(gridMap.GetLength(0), gridMap.GetLength(1)));
    }

    // Create the grid cell objects
    GridCellData[,] CreateMeshData(int[,] gridMap)
    {
        int gridWidth = gridMap.GetLength(0);
        int gridHeight = gridMap.GetLength(1);

        GridCellData[,] gridCellsData = new GridCellData[gridWidth, gridHeight];

        // Grid displacement so the middle is directly underneath camera
        float gridDisplacementX = (float)(0.5 * (gridWidth * cellSize));
        float gridDisplacementZ = (float)(0.5 * (gridHeight * cellSize));


        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                float posX = i * cellSize - gridDisplacementX;
                float posZ = j * cellSize - gridDisplacementZ;

                gridCellsData[i, j] = CreateGridCellData(new Vector3(posX, 0, posZ));
            }
        }

        return gridCellsData;
    }

    // Creates a grid cell at a certain position
    GridCellData CreateGridCellData(Vector3 pos)
    {

        Vector3[] vertices = new Vector3[4];

        Color[] colors = new Color[4];

        Vector3[] normals = new Vector3[4];

        // Two triangles that make up the grid cell mesh
        int[] triangles = new int[6];


        Vector3 lb = pos;
        Vector3 ul = pos + Vector3.forward;
        Vector3 rb = pos + Vector3.right;
        Vector3 ur = pos + Vector3.forward + Vector3.right;

        vertices[0] = lb;
        vertices[1] = ul;
        vertices[2] = rb;
        vertices[3] = ur;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;

        normals[0] = Vector3.up;
        normals[1] = Vector3.up;
        normals[2] = Vector3.up;
        normals[3] = Vector3.up;

        return new GridCellData(vertices, triangles, colors, normals);
    }
    
    // Creates the mesh
    void CreateMesh(GridCellData[,] MeshData)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector3> normals = new List<Vector3>();

        foreach (GridCellData gridCelldata in MeshData)
        {
            foreach (int triangle in gridCelldata.triangles)
            {
                Vector3 vertex = gridCelldata.vertices[triangle];
                Vector3 normal = gridCelldata.normals[triangle];
                int vertexNum = vertices.Count;
                vertices.Add(vertex);
                triangles.Add(vertexNum);
                normals.Add(normal);
            }
        }

        Mesh gridMesh = new Mesh();
        gridMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = gridMesh;
        gridMesh.vertices = vertices.ToArray();
        gridMesh.triangles = triangles.ToArray();
        gridMesh.normals = normals.ToArray();

        meshColors = new Color[vertices.Count];
    }

    // Updates the mesh
    void UpdateMesh(int[,] gridMap)
    {
        int index = 0;
        for (int i = 0; i < gridMap.GetLength(0); i++)
        {
            for (int j = 0; j < gridMap.GetLength(1); j++)
            {
                if (gridMap[i, j] == 1)
                {
                    meshColors[index * 6] = new Color(1, 1, 1);
                    meshColors[index * 6 + 1] = new Color(1, 1, 1);
                    meshColors[index * 6 + 2] = new Color(1, 1, 1);
                    meshColors[index * 6 + 3] = new Color(1, 1, 1);
                    meshColors[index * 6 + 4] = new Color(1, 1, 1);
                    meshColors[index * 6 + 5] = new Color(1, 1, 1);
                }

                else
                {
                    meshColors[index * 6] = new Color(0, 0, 0);
                    meshColors[index * 6 + 1] = new Color(0, 0, 0);
                    meshColors[index * 6 + 2] = new Color(0, 0, 0);
                    meshColors[index * 6 + 3] = new Color(0, 0, 0);
                    meshColors[index * 6 + 4] = new Color(0, 0, 0);
                    meshColors[index * 6 + 5] = new Color(0, 0, 0);
                }
                index++;

            }
        }

        meshFilter.mesh.colors = meshColors;
    }
}
