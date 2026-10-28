using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Play 중에 직접 돌아다니는 카메라
// 이동: WASD / 위아래: E, Q / 둘러보기: 마우스 오른쪽 버튼 누른 채 드래그
// 빠르게: Shift / 스크린샷: P
public class FreeCamera : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float lookSpeed = 0.15f;

    float yaw, pitch;

    void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;
        Vector2 look = Vector2.zero;
        bool fast = false, rightMouse = false, shot = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed) move.z += 1;
            if (kb.sKey.isPressed) move.z -= 1;
            if (kb.dKey.isPressed) move.x += 1;
            if (kb.aKey.isPressed) move.x -= 1;
            if (kb.eKey.isPressed) move.y += 1;
            if (kb.qKey.isPressed) move.y -= 1;
            fast = kb.leftShiftKey.isPressed;
            shot = kb.pKey.wasPressedThisFrame;
        }
        if (mouse != null)
        {
            rightMouse = mouse.rightButton.isPressed;
            look = mouse.delta.ReadValue();
        }
#else
        if (Input.GetKey(KeyCode.W)) move.z += 1;
        if (Input.GetKey(KeyCode.S)) move.z -= 1;
        if (Input.GetKey(KeyCode.D)) move.x += 1;
        if (Input.GetKey(KeyCode.A)) move.x -= 1;
        if (Input.GetKey(KeyCode.E)) move.y += 1;
        if (Input.GetKey(KeyCode.Q)) move.y -= 1;
        fast = Input.GetKey(KeyCode.LeftShift);
        shot = Input.GetKeyDown(KeyCode.P);
        rightMouse = Input.GetMouseButton(1);
        look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10f;
#endif

        // 마우스 오른쪽 버튼을 누르고 있을 때만 둘러보기
        if (rightMouse)
        {
            yaw += look.x * lookSpeed;
            pitch -= look.y * lookSpeed;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // 카메라가 바라보는 방향 기준으로 이동
        float speed = moveSpeed * (fast ? 3f : 1f);
        transform.Translate(move * speed * Time.deltaTime, Space.Self);

        // P: 스크린샷 저장 (프로젝트 폴더 안 Screenshots 폴더)
        if (shot)
        {
            Directory.CreateDirectory("Screenshots");
            string path = "Screenshots/mid_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("스크린샷 저장: " + path);
        }
    }
}