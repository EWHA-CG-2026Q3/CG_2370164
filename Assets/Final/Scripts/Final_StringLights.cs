using System.Collections.Generic;
using UnityEngine;

// 두 점 사이를 De Casteljau 곡선(2차 베지어)으로 잇고, 곡선 위에 전구를 단다
// 비어 있는 칸(시작점, 끝점, 재질)은 Play를 시작할 때 자동으로 채운다
[RequireComponent(typeof(LineRenderer))]
public class Final_StringLights : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float sag = 0.7f;          // 가운데가 처지는 정도
    public int segments = 30;         // 곡선을 몇 조각으로 나눌지
    public int bulbCount = 12;
    public float bulbSize = 0.1f;
    public float lineWidth = 0.02f;   // 줄 굵기
    public Material bulbMaterial;
    public Material lineMaterial;
    public float swayAmount = 0.08f;  // 흔들리는 폭
    public float swaySpeed = 0.8f;    // 흔들리는 빠르기

    LineRenderer line;
    List<Transform> bulbs = new List<Transform>();

    void Start()
    {
        // 시작점, 끝점이 비어 있으면 자식에서 찾고, 없으면 카운터 위 공중에 만든다
        if (startPoint == null) startPoint = FindOrMakePoint("Start", new Vector3(-4.5f, 3.6f, 1.0f));
        if (endPoint == null) endPoint = FindOrMakePoint("End", new Vector3(4.5f, 3.6f, 1.0f));

        // 줄 설정
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        if (lineMaterial == null) lineMaterial = MakeMaterial(new Color(0.08f, 0.08f, 0.08f), Color.black);
        line.sharedMaterial = lineMaterial;

        // 전구 재질이 비어 있으면 주황빛으로 빛나는 재질을 만든다
        if (bulbMaterial == null) bulbMaterial = MakeMaterial(new Color(1f, 0.7f, 0.35f), new Color(1f, 0.55f, 0.2f) * 4f);

        // 전구를 반복문으로 만들어 자식으로 붙임
        for (int i = 0; i < bulbCount; i++)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.name = "Bulb" + i;
            b.transform.SetParent(transform, false);
            b.transform.localScale = Vector3.one * bulbSize;
            Destroy(b.GetComponent<Collider>());
            b.GetComponent<MeshRenderer>().sharedMaterial = bulbMaterial;
            bulbs.Add(b.transform);
        }
    }

    Transform FindOrMakePoint(string pointName, Vector3 position)
    {
        Transform found = transform.Find(pointName);
        if (found != null) return found;
        GameObject g = new GameObject(pointName);
        g.transform.SetParent(transform, false);
        g.transform.position = position;
        return g.transform;
    }

    Material MakeMaterial(Color baseColor, Color emission)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", baseColor);
        if (emission.maxColorComponent > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
        }
        return m;
    }

    // De Casteljau: 선분의 t 지점을 반복해서 줄여 가며 곡선 위의 점을 구함
    Vector3 DeCasteljau(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);   // 1단계: (1-t)·P0 + t·P1
        Vector3 b = Vector3.Lerp(p1, p2, t);   //        (1-t)·P1 + t·P2
        return Vector3.Lerp(a, b, t);          // 2단계: (1-t)·a + t·b
    }

    void Update()
    {
        if (line == null || startPoint == null || endPoint == null) return;

        Vector3 p0 = startPoint.position;
        Vector3 p2 = endPoint.position;
        Vector3 mid = (p0 + p2) * 0.5f;

        // 2차 베지어는 가운데(t=0.5)가 제어점 쪽으로 절반만 가므로, 처짐의 2배만큼 내림
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        Vector3 p1 = mid + Vector3.down * sag * 2f + Vector3.forward * sway;

        // 곡선 그리기
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            line.SetPosition(i, DeCasteljau(p0, p1, p2, t));
        }

        // 전구 달기 (양 끝은 빼고 고르게)
        for (int i = 0; i < bulbs.Count; i++)
        {
            float t = (i + 1f) / (bulbs.Count + 1f);
            bulbs[i].position = DeCasteljau(p0, p1, p2, t) + Vector3.down * bulbSize * 0.6f;
        }
    }
}
