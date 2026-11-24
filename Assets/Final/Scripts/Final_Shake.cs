using UnityEngine;

// 칵테일 셰이커가 몇 초마다 한 번씩 좌우로 흔들린다 (회전 + 위아래 이동)
public class Final_Shake : MonoBehaviour
{
    public float angle = 12f;      // 최대 기울기 (도)
    public float speed = 18f;      // 흔드는 빠르기
    public float interval = 4f;    // 몇 초마다 흔들지
    public float duration = 1.2f;  // 한 번에 몇 초 동안 흔들지
    public float lift = 0.02f;     // 흔들 때 들썩이는 높이

    Quaternion baseRotation;
    Vector3 basePosition;

    void Start()
    {
        baseRotation = transform.localRotation;
        basePosition = transform.localPosition;
    }

    void Update()
    {
        float t = Time.time % interval;
        // 흔드는 동안에만 0 → 1 → 0 으로 세기가 변함
        float strength = t < duration ? Mathf.Sin(t / duration * Mathf.PI) : 0f;
        float a = Mathf.Sin(Time.time * speed) * angle * strength;

        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, a);
        transform.localPosition = basePosition + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * speed)) * lift * strength;
    }
}
