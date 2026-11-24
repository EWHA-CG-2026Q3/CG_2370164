using System.Collections.Generic;
using UnityEngine;

// 와인잔의 옆모습(받침, 다리, 몸통)을 점 목록으로 만들고, y축으로 한 바퀴 돌려서 곡면 메시를 만든다
// 몸통 곡선은 3차 베지어를 De Casteljau 방식으로 계산한다
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Final_WineGlass : MonoBehaviour
{
    [Range(3, 64)] public int segments = 32;     // 둘레를 몇 조각으로 나눌지
    [Range(2, 40)] public int curveSteps = 16;   // 몸통 곡선을 몇 조각으로 나눌지

    [Header("받침, 다리")]
    public float baseRadius = 0.035f;
    public float baseHeight = 0.004f;
    public float stemRadius = 0.004f;
    public float stemHeight = 0.07f;

    [Header("몸통 베지어 제어점 (x = 반지름, y = 높이)")]
    public Vector2 bowlP1 = new Vector2(0.05f, 0.075f);
    public Vector2 bowlP2 = new Vector2(0.05f, 0.14f);
    public Vector2 bowlP3 = new Vector2(0.035f, 0.17f);   // 잔 입구

    bool needRebuild = true;

    // Inspector에서 값을 바꾸면 다시 만들라고 표시
    void OnValidate()
    {
        needRebuild = true;
    }

    void Update()
    {
        if (needRebuild)
        {
            needRebuild = false;
            Build();
        }
    }

    // De Casteljau: 선분의 t 지점을 반복해서 줄여 가며 곡선 위의 점을 구함 (점 4개 → 3 → 2 → 1)
    Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 a = Vector2.Lerp(p0, p1, t);
        Vector2 b = Vector2.Lerp(p1, p2, t);
        Vector2 c = Vector2.Lerp(p2, p3, t);
        Vector2 d = Vector2.Lerp(a, b, t);
        Vector2 e = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(d, e, t);
    }

    // 에디터 도구에서 값을 바꾼 뒤 다시 만들 때 사용
    public void Rebuild()
    {
        Build();
    }

    void Build()
    {
        // 1) 옆모습 점 목록 (x = 반지름, y = 높이), 아래에서 위로
        List<Vector2> profile = new List<Vector2>();
        profile.Add(new Vector2(0f, 0f));                              // 받침 바닥 가운데
        profile.Add(new Vector2(baseRadius, 0f));                      // 받침 가장자리
        profile.Add(new Vector2(baseRadius, baseHeight));              // 받침 두께
        profile.Add(new Vector2(stemRadius, baseHeight + 0.002f));     // 다리 시작

        Vector2 p0 = new Vector2(stemRadius, baseHeight + stemHeight); // 다리 끝 = 몸통 곡선 시작
        profile.Add(p0);
        for (int i = 1; i <= curveSteps; i++)
        {
            float t = (float)i / curveSteps;
            profile.Add(Bezier(p0, bowlP1, bowlP2, bowlP3, t));
        }

        // 2) 회전: 옆모습의 점마다 y축으로 돌려서 원(링)을 만듦
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int n = segments;
        int rings = profile.Count;

        for (int k = 0; k < rings; k++)
        {
            float r = profile[k].x;
            float y = profile[k].y;
            for (int i = 0; i < n; i++)
            {
                float a = 2f * Mathf.PI * i / n;   // 360° / n * i
                vertices.Add(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r));
            }
        }

        // 3) 이웃한 두 링을 사각형(삼각형 2개)으로 이어서 곡면을 채움
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

        Mesh mesh = new Mesh();
        mesh.name = "WineGlass";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
