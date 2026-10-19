
using UnityEngine;
using UnityEngine.InputSystem;

public class PCCameraController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("旋轉設定")]
    [SerializeField] private float rotationSpeed = 0.5f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;

        if (pitch > 180f)
            pitch -= 360f;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        Vector3 direction = Vector3.zero;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        if (keyboard.wKey.isPressed)
            direction += forward;

        if (keyboard.sKey.isPressed)
            direction -= forward;

        if (keyboard.aKey.isPressed)
            direction -= right;

        if (keyboard.dKey.isPressed)
            direction += right;

        direction = Vector3.ClampMagnitude(direction, 1f);

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.rightButton.isPressed)
            return;

        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * rotationSpeed;
        pitch -= delta.y * rotationSpeed;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
