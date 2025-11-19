using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 200f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    public Transform spork;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Rotate player to face world X-axis
        transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
    }

    void Update()
    {
        Move();
        Look();
    }

    void Move()
    {
        // Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void ResetCameraRotation(float pitch = -0f, float yaw = 90f)
    {
        // Reset player yaw
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Reset camera pitch
        xRotation = pitch;
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

}
