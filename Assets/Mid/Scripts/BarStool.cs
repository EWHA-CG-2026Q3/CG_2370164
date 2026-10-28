using System.Collections.Generic;
using UnityEngine;

// 반복문으로 n각기둥 3개(받침, 기둥, 좌석)를 쌓아 바 스툴 메시를 만든다
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BarStool : MonoBehaviour
{
    [Range(3, 64)] public int segments = 24;   // 원을 몇 각형으로 나눌지

    [Header("받침")]
    public float baseRadius = 0.22f;
    public float baseHeight = 0.03f;

    [Header("기둥")]
    public float poleRadius = 0.04f;
    public float poleHeight = 0.7f;

    [Header("좌석")]
    public float seatRadius = 0.25f;
    public float seatHeight = 0.08f;

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

        float y = 0f;
        AddCylinder(y, baseRadius, baseHeight); y += baseHeight;  // 받침
        AddCylinder(y, poleRadius, poleHeight); y += poleHeight;  // 기둥
        AddCylinder(y, seatRadius, seatHeight);                   // 좌석

        Mesh mesh = new Mesh();
        mesh.name = "BarStool";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    // 높이 y0에서 시작하는 반지름 r, 높이 h의 n각기둥을 추가
    void AddCylinder(float y0, float r, float h)
    {
        float y1 = y0 + h;
        int n = segments;

        // 1) 옆면: 원 위의 점 i마다 아래 점과 위 점을 하나씩 만든다
        int side = vertices.Count;
        for (int i = 0; i < n; i++)
        {
            float a = 2f * Mathf.PI * i / n;          // 360° / n * i
            float x = Mathf.Cos(a) * r;
            float z = Mathf.Sin(a) * r;
            vertices.Add(new Vector3(x, y0, z));      // 아래 점: side + 2i
            vertices.Add(new Vector3(x, y1, z));      // 위 점:   side + 2i + 1
        }
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;                   // 마지막 점은 첫 점과 이어진다
            int b0 = side + 2 * i,    t0 = b0 + 1;
            int b1 = side + 2 * next, t1 = b1 + 1;
            triangles.Add(b0); triangles.Add(t0); triangles.Add(b1);
            triangles.Add(t0); triangles.Add(t1); triangles.Add(b1);
        }

        // 2) 윗면과 아랫면: 가운데 점 + 원 위의 이웃한 두 점
        AddCap(y1, r, true);
        AddCap(y0, r, false);
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
            // 삼각형 순서에 따라 보이는 면이 정해진다 (윗면은 위에서, 아랫면은 아래에서 보임)
            if (up) { triangles.Add(center); triangles.Add(next); triangles.Add(cur); }
            else    { triangles.Add(center); triangles.Add(cur);  triangles.Add(next); }
        }
    }
}
