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

    [Header("Grab Settings")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private float grabMoveSpeed = 10f;

    [Header("Highlight Settings")]
    [SerializeField] private Material highlightMaterial;

    private Transform currentTarget;
    private Material originalMaterial;

    private Rigidbody grabbedObject;
    private Vector3 grabOffset;



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
        if (Mouse.current == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        bool hitSomething = Physics.Raycast(ray, out hit, rayDistance, interactableLayer);

        // ===== HIGHLIGHT LOGIC =====
        if (hitSomething)
        {
            Transform target = hit.collider.transform;

            if (currentTarget != target)
            {
                ClearHighlight();

                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterial = renderer.material;
                    renderer.material = highlightMaterial;
                    currentTarget = target;
                }
            }
        }
        else
        {
            ClearHighlight();
        }

        // ===== GRAB LOGIC =====
        if (Mouse.current.leftButton.wasPressedThisFrame && hitSomething)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                grabbedObject = rb;
                grabbedObject.useGravity = false;
                grabbedObject.linearDamping = 10f;
            }
        }

        if (Mouse.current.leftButton.isPressed && grabbedObject != null)
        {
            Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * grabDistance;
            Vector3 direction = targetPosition - grabbedObject.position;
            grabbedObject.linearVelocity = direction * grabMoveSpeed;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && grabbedObject != null)
        {

            grabbedObject.useGravity = true;
            grabbedObject.linearDamping = 0f;
            grabbedObject = null;
        }
    }

    private void ClearHighlight()
    {
        if (currentTarget != null)
        {
            Renderer renderer = currentTarget.GetComponent<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }

            currentTarget = null;
            originalMaterial = null;
        }
    }

    public void ForceRelease(Rigidbody rb)
    {
        if (grabbedObject == rb)
        {
            grabbedObject.linearVelocity = Vector3.zero;
            grabbedObject.angularVelocity = Vector3.zero;

            grabbedObject.useGravity = true;
            grabbedObject.linearDamping = 0f;

            grabbedObject = null;

            ClearHighlight();
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