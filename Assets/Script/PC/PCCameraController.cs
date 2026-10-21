using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PCCameraController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("旋轉設定")]
    [SerializeField] private float rotationSpeed = 0.5f;

    [Header("重力設定")]
    [SerializeField] private float gravity = -9.81f;

    private CharacterController characterController;

    private float yaw;
    private float pitch;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

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

        Vector3 direction = Vector3.zero;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed)
                direction += forward;

            if (keyboard.sKey.isPressed)
                direction -= forward;

            if (keyboard.aKey.isPressed)
                direction -= right;

            if (keyboard.dKey.isPressed)
                direction += right;
        }

        direction = Vector3.ClampMagnitude(direction, 1f);

        Vector3 horizontalMovement = direction * moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 movement = horizontalMovement + Vector3.up * verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
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