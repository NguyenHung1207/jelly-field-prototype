using System.Collections.Generic;
using UnityEngine;

public static class RoundedCellMeshFactory
{
    public static Mesh CreateRoundedSquareMesh(float size, float radius, int cornerSegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Rounded Square Cell Mesh";

        float half = size * 0.5f;
        radius = Mathf.Clamp(radius, 0.01f, half - 0.01f);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(Vector3.zero);

        AddCorner(vertices, half - radius, half - radius, radius, 0f, 90f, cornerSegments);
        AddCorner(vertices, -half + radius, half - radius, radius, 90f, 180f, cornerSegments);
        AddCorner(vertices, -half + radius, -half + radius, radius, 180f, 270f, cornerSegments);
        AddCorner(vertices, half - radius, -half + radius, radius, 270f, 360f, cornerSegments);

        int perimeterCount = vertices.Count - 1;

        for (int i = 0; i < perimeterCount; i++)
        {
            int current = i + 1;
            int next = i == perimeterCount - 1 ? 1 : current + 1;

            triangles.Add(0);
            triangles.Add(next);
            triangles.Add(current);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void AddCorner(
        List<Vector3> vertices,
        float centerX,
        float centerZ,
        float radius,
        float startAngle,
        float endAngle,
        int segments)
    {
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

            float x = centerX + Mathf.Cos(angle) * radius;
            float z = centerZ + Mathf.Sin(angle) * radius;

            vertices.Add(new Vector3(x, 0f, z));
        }
    }
}