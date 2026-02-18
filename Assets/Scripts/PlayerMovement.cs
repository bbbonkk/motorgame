using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputAction moveAction;

    private InputAction lookAction;

    private Vector2 move;

    private Vector2 look;

    private Rigidbody rb;

    public float m_movementSpeed = 5.0f;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        move = moveAction.ReadValue<Vector2>();
        look = lookAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Walk();
        Look();
    }

    private void Walk()
    {
        Vector3 direction = transform.forward * move.y + transform.right * move.x;

        rb.MovePosition(rb.position + direction * m_movementSpeed * Time.deltaTime);
    }

    private void Look()
    {
        
    }
}
