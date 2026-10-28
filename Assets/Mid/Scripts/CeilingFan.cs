using UnityEngine;

// 허브(부모)에 날개(자식)를 반복문으로 붙이고, 허브만 코드로 회전시킨다
public class CeilingFan : MonoBehaviour
{
    public int bladeCount = 4;
    public float bladeLength = 0.7f;
    public float speed = 120f;          // 초당 회전 각도
    public Material bladeMaterial;

    float angle = 0f;

    void Start()
    {
        for (int i = 0; i < bladeCount; i++)
        {
            float a = 360f / bladeCount * i;

            // 1) 팔: 허브의 자식, y축으로 a도 회전
            GameObject arm = new GameObject("Arm" + i);
            arm.transform.SetParent(transform, false);
            arm.transform.localRotation = Quaternion.Euler(0f, a, 0f);

            // 2) 날개: 팔의 자식, 팔 방향으로 밀어내고 살짝 기울임
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade" + i;
            blade.transform.SetParent(arm.transform, false);
            blade.transform.localPosition = new Vector3(bladeLength * 0.5f + 0.1f, 0f, 0f);
            blade.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            blade.transform.localScale = new Vector3(bladeLength, 0.02f, 0.15f);
            Destroy(blade.GetComponent<Collider>());

            if (bladeMaterial != null)
                blade.GetComponent<MeshRenderer>().sharedMaterial = bladeMaterial;
        }
    }

    void Update()
    {
        // 부모(허브)만 회전 → 자식(팔, 날개)이 함께 돈다
        angle += speed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
}