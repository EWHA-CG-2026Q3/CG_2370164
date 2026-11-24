using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 잔을 마우스 왼쪽 버튼으로 클릭하면 와인이 차오르고, 꽉 찬 잔을 다시 클릭하면 비워진다
// 셰이더(Final_Glass)의 _FillHeight 값을 코드로 바꿔서 술 높이를 움직인다
public class Final_ClickFill : MonoBehaviour
{
    public float fullHeight = 0.135f;   // 꽉 찼을 때 술 높이 (오브젝트 좌표)
    public float emptyHeight = 0.08f;   // 비었을 때 (= 재질의 Fill Bottom)
    public float speed = 0.03f;         // 1초에 차오르는 높이

    Material mat;
    float current;
    float target;

    void Start()
    {
        mat = GetComponent<Renderer>().material;   // 이 잔만 바뀌도록 재질 복사본을 씀
        current = mat.GetFloat("_FillHeight");
        target = current;
    }

    void Update()
    {
        Vector2 screenPos;
        if (ClickedThisFrame(out screenPos))
        {
            Camera cam = Camera.main;
            RaycastHit hit;
            // 클릭한 화면 위치에서 광선을 쏴서 이 잔에 맞았는지 확인
            if (cam != null && Physics.Raycast(cam.ScreenPointToRay(screenPos), out hit, 100f)
                && hit.collider.gameObject == gameObject)
            {
                bool isFull = target >= fullHeight - 0.001f;
                target = isFull ? emptyHeight : fullHeight;
            }
        }

        current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
        mat.SetFloat("_FillHeight", current);
    }

    bool ClickedThisFrame(out Vector2 pos)
    {
#if ENABLE_INPUT_SYSTEM
        pos = Vector2.zero;
        if (Mouse.current == null) return false;
        pos = Mouse.current.position.ReadValue();
        return Mouse.current.leftButton.wasPressedThisFrame;
#else
        pos = Input.mousePosition;
        return Input.GetMouseButtonDown(0);
#endif
    }
}
