using UnityEngine;
using UnityEngine.Rendering;

// GL 클래스로 "BAR"와 칵테일잔을 RenderTexture에 그리고, 그 텍스처를 간판 판의 Emission으로 쓴다
public class NeonSign : MonoBehaviour
{
    public Renderer board;                                  // 텍스처를 입힐 간판 판(Quad)
    public Color textColor = new Color(1f, 0.2f, 0.6f);    // 분홍
    public Color glassColor = new Color(0.2f, 0.9f, 1f);   // 하늘색
    public float lineWidth = 0.06f;
    public bool flicker = true;

    RenderTexture rt;
    Material glMat;

    // 선분 목록: 점 2개씩 한 선분 (글자 높이 1 기준)
    static readonly Vector2[] text =
    {
        // B
        new Vector2(0f, 0f),     new Vector2(0f, 1f),
        new Vector2(0f, 1f),     new Vector2(0.4f, 1f),
        new Vector2(0.4f, 1f),   new Vector2(0.5f, 0.9f),
        new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.6f),
        new Vector2(0.5f, 0.6f), new Vector2(0.4f, 0.5f),
        new Vector2(0.4f, 0.5f), new Vector2(0f, 0.5f),
        new Vector2(0.4f, 0.5f), new Vector2(0.5f, 0.4f),
        new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.1f),
        new Vector2(0.5f, 0.1f), new Vector2(0.4f, 0f),
        new Vector2(0.4f, 0f),   new Vector2(0f, 0f),
        // A
        new Vector2(0.8f, 0f),   new Vector2(1.05f, 1f),
        new Vector2(1.05f, 1f),  new Vector2(1.3f, 0f),
        new Vector2(0.9f, 0.4f), new Vector2(1.2f, 0.4f),
        // R
        new Vector2(1.6f, 0f),   new Vector2(1.6f, 1f),
        new Vector2(1.6f, 1f),   new Vector2(2.0f, 1f),
        new Vector2(2.0f, 1f),   new Vector2(2.1f, 0.9f),
        new Vector2(2.1f, 0.9f), new Vector2(2.1f, 0.6f),
        new Vector2(2.1f, 0.6f), new Vector2(2.0f, 0.5f),
        new Vector2(2.0f, 0.5f), new Vector2(1.6f, 0.5f),
        new Vector2(1.8f, 0.5f), new Vector2(2.1f, 0f),
    };

    static readonly Vector2[] glass =
    {
        new Vector2(2.5f, 1f),    new Vector2(3.1f, 1f),
        new Vector2(3.1f, 1f),    new Vector2(2.8f, 0.55f),
        new Vector2(2.8f, 0.55f), new Vector2(2.5f, 1f),
        new Vector2(2.8f, 0.55f), new Vector2(2.8f, 0f),
        new Vector2(2.6f, 0f),    new Vector2(3.0f, 0f),
    };

    void Start()
    {
        rt = new RenderTexture(1100, 400, 0);
        rt.Create();

        glMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        glMat.SetInt("_Cull", (int)CullMode.Off);
        glMat.SetInt("_ZWrite", 0);
        glMat.SetInt("_ZTest", (int)CompareFunction.Always);

        // 간판 판 재질의 Emission 텍스처로 연결
        board.material.SetTexture("_EmissionMap", rt);
        board.material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // 글자만 가끔 어두워지게 (고장 난 네온 느낌)
        float k = (flicker && Mathf.PerlinNoise(Time.time * 3f, 0f) < 0.25f) ? 0.3f : 1f;
        Color tc = textColor * k;
        tc.a = 1f;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;                 // 이제부터 GL은 rt에 그린다

        GL.PushMatrix();
        GL.LoadPixelMatrix(-0.1f, 3.2f, -0.1f, 1.1f);   // 좌표 범위를 텍스처 전체에 맞춤
        GL.Clear(true, true, Color.black);
        glMat.SetPass(0);
        GL.Begin(GL.QUADS);
        DrawSegments(text, tc);
        DrawSegments(glass, glassColor);
        GL.End();
        GL.PopMatrix();

        RenderTexture.active = prev;
    }

    // 선분 하나를 굵기 lineWidth의 사각형으로 그린다
    void DrawSegments(Vector2[] pts, Color c)
    {
        GL.Color(c);
        float h = lineWidth * 0.5f;
        for (int i = 0; i + 1 < pts.Length; i += 2)
        {
            Vector2 a = pts[i], b = pts[i + 1];
            Vector2 d = (b - a).normalized;          // 선분 방향
            Vector2 n = new Vector2(-d.y, d.x) * h;  // 수직 방향 × 굵기 절반
            GL.Vertex3(a.x + n.x, a.y + n.y, 0f);
            GL.Vertex3(b.x + n.x, b.y + n.y, 0f);
            GL.Vertex3(b.x - n.x, b.y - n.y, 0f);
            GL.Vertex3(a.x - n.x, a.y - n.y, 0f);
        }
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}