using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Color[] colors;
    public Vector3[] normals;

    public GridCellData(Vector3[] vertices, int[] triangles, Color[] colors, Vector3[] normals)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.colors = colors;
        this.normals = normals;
    }
}