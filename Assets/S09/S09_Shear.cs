using UnityEngine;

// S09 Shear 실습
// (x, y, z) -> (x + k*y, y, z)
// 학번 끝자리 4 -> k = (4 + 1) / 5 = 1
[ExecuteAlways]
[RequireComponent(typeof(DiamondMesh))]
public class S09_Shear : MonoBehaviour
{
    [SerializeField] float k = 1f;

    DiamondMesh diamondMesh;

    void OnEnable()
    {
        diamondMesh = GetComponent<DiamondMesh>();
    }

    void Update()
    {
        if (diamondMesh == null || diamondMesh.BaseVertices == null) return;

        float[,] S = ShearMatrixRaw(k);

        Vector3[] baseVertices = diamondMesh.BaseVertices;
        Vector3[] verts = new Vector3[baseVertices.Length];

        for (int i = 0; i < baseVertices.Length; i++)
        {
            verts[i] = FromHomogeneous(
                MultiplyMatrixVectorRaw(
                    S,
                    ToHomogeneous(baseVertices[i])
                )
            );
        }

        diamondMesh.SetVertices(verts);

        //top vertex 결과 확인
        Vector3 topVertex = new Vector3(0.5f, 1f, 0.5f);
        Vector3 transformedTop = FromHomogeneous(
            MultiplyMatrixVectorRaw(
                S,
                ToHomogeneous(topVertex)
            )
        );

        Debug.Log($"k = {k}, top vertex = {transformedTop}");
    }

    // Shear 행렬
    // (x, y, z) -> (x + k*y, y, z)
    float[,] ShearMatrixRaw(float k)
    {
        return new float[,] {
            { 1f, k,  0f, 0f },
            { 0f, 1f, 0f, 0f },
            { 0f, 0f, 1f, 0f },
            { 0f, 0f, 0f, 1f }
        };
    }

    Vector4 ToHomogeneous(Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 1f);
    }

    Vector3 FromHomogeneous(Vector4 h)
    {
        return new Vector3(h.x, h.y, h.z);
    }

    Vector4 MultiplyMatrixVectorRaw(float[,] M, Vector4 v)
    {
        float[] input = { v.x, v.y, v.z, v.w };
        float[] result = new float[4];

        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
                result[row] += M[row, col] * input[col];

        return new Vector4(
            result[0],
            result[1],
            result[2],
            result[3]
        );
    }
}