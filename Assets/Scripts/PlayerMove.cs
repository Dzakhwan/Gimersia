using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private InputSystem playerInput;
    [SerializeField] private Vector2 moveInput;

    void Awake()
    {
        playerInput = new InputSystem();
    }

    void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.Move.performed += Move;

    }

    void OnDisable()
    {
        playerInput.Disable();
        playerInput.Player.Move.performed -= Move;
    }

    void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("gerak euy");
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0);
        transform.Translate(move * Time.fixedDeltaTime * 5f);
    }
}