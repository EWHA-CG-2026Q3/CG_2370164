using System.Collections.Generic;
using UnityEngine;

// 병의 옆모습(높이별 반지름)을 따라 반복문으로 링을 만들고 이어서 병 메시를 만든다
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Bottle : MonoBehaviour
{
    [Range(3, 64)] public int segments = 24;

    // (반지름, 높이)를 아래에서 위 순서로
    public Vector2[] profile =
    {
        new Vector2(0.07f,  0.00f),
        new Vector2(0.07f,  0.22f),
        new Vector2(0.05f,  0.27f),
        new Vector2(0.025f, 0.31f),
        new Vector2(0.025f, 0.40f),
        new Vector2(0.03f,  0.41f),
        new Vector2(0.03f,  0.43f),
    };

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    void Start()
    {
        Build();
    }

    void Build()
    {
        vertices.Clear();
        triangles.Clear();
        int n = segments;
        int rings = profile.Length;

        // 1) 링: profile의 높이마다 원 위에 점 n개
        for (int k = 0; k < rings; k++)
        {
            float r = profile[k].x;
            float y = profile[k].y;
            for (int i = 0; i < n; i++)
            {
                float a = 2f * Mathf.PI * i / n;      // 360° / n * i
                vertices.Add(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r));
            }
        }

        // 2) 옆면: 링 k와 링 k+1을 사각형(삼각형 2개)으로 잇기
        for (int k = 0; k < rings - 1; k++)
        {
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                int b0 = k * n + i,       b1 = k * n + next;
                int t0 = (k + 1) * n + i, t1 = (k + 1) * n + next;
                triangles.Add(b0); triangles.Add(t0); triangles.Add(b1);
                triangles.Add(t0); triangles.Add(t1); triangles.Add(b1);
            }
        }

        // 3) 바닥과 입구 막기
        AddCap(profile[0].y, profile[0].x, false);
        AddCap(profile[rings - 1].y, profile[rings - 1].x, true);

        Mesh mesh = new Mesh();
        mesh.name = "Bottle";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void AddCap(float y, float r, bool up)
    {
        int n = segments;
        int center = vertices.Count;
        vertices.Add(new Vector3(0f, y, 0f));
        for (int i = 0; i < n; i++)
        {
            float a = 2f * Mathf.PI * i / n;
            vertices.Add(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r));
        }
        for (int i = 0; i < n; i++)
        {
            int cur = center + 1 + i;
            int next = center + 1 + (i + 1) % n;
            if (up) { triangles.Add(center); triangles.Add(next); triangles.Add(cur); }
            else    { triangles.Add(center); triangles.Add(cur);  triangles.Add(next); }
        }
    }
}