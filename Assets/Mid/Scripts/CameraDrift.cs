using UnityEngine;

// 카메라가 좌우로 천천히 오가며 항상 카운터를 바라본다
public class CameraDrift : MonoBehaviour
{
    public Vector3 lookTarget = new Vector3(0f, 1.3f, 2f);  // 바라볼 지점
    public float range = 1.5f;     // 좌우로 움직이는 폭
    public float speed = 0.3f;     // 움직이는 빠르기

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * range;
        transform.position = startPos + new Vector3(x, 0f, 0f);
        transform.LookAt(lookTarget);
    }
}