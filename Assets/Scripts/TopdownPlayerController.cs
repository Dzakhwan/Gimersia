using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopdownPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private InputSystem playerInput;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        playerInput = new InputSystem();
    }

    private void OnEnable()
    {
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Disable();
        moveInput = Vector2.zero;
    }

    private void OnDestroy()
    {
        playerInput?.Dispose();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = Vector2.ClampMagnitude(moveInput, 1f) * moveSpeed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
}
