using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -60f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 10f; // How far the ray reaches
    [SerializeField] private LayerMask interactableLayer; // Optional: filter what you hit

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float verticalRotation = 0f;
    private bool isCursorLocked = true;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("PlayerController: CharacterController component not found!");
        }

        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
        }
    }

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        HandleCursorLock();

        if (isCursorLocked)
        {
            HandleMovement();
            HandleMouseLook();
            HandleRaycast(); // Added call here
        }
    }

    private void HandleRaycast()
    {
        // Changed "wasPressedThisFrame" to "isPressed" for continuous firing
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // The ray starts at the camera position and fires forward
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            // Visualizing the continuous beam in the Scene view
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * rayDistance, Color.green);

            if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
            {
                // This will now log every frame while you are pointing at something
                Debug.Log("Continuously hitting: " + hit.collider.name);

                // Example: If you wanted to "drill" or "heal" something over time:
                // hit.collider.GetComponent<Health>()?.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    private void HandleCursorLock()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isCursorLocked)
            {
                UnlockCursor();
            }
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    private void HandleMouseLook()
    {
        if (Mouse.current == null || cameraTransform == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
        }

        moveInput = moveInput.normalized;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.TransformDirection(move);
        characterController.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}