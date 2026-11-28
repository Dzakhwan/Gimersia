using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Water Movement")]
    [SerializeField] private float swimSpeed = 5.5f;
    [SerializeField] private float waterDamping = 4f;
    [SerializeField] private float waterGravityScale = 0f;
    [SerializeField] private float dolphinJumpForce = 12.5f;
    [SerializeField] private float dolphinJumpGraceDuration = 0.22f;
    [SerializeField] private float surfaceSubmergeDepth = 0.22f;

    [Header("Land Movement")]
    [SerializeField] private float landMoveSpeed = 2.6f;
    [SerializeField] private float landDamping = 0.5f;
    [SerializeField] private float landGravityScale = 2.6f;
    [SerializeField] private float landJumpForce = 9.2f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.15f);
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.36f, 0.08f);

    public event Action OnLandJump;
    public event Action OnDolphinJump;

    public bool IsInWater { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsMoving => Mathf.Abs(moveInput.x) > 0.01f || (IsInWater && Mathf.Abs(moveInput.y) > 0.01f);
    public int FacingDirection { get; private set; } = 1;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

    private readonly List<WaterZone> activeWaterZones = new List<WaterZone>();
    private InputSystem playerInput;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpQueued;
    private float waterJumpGraceTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerInput = new InputSystem();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Jump.performed -= OnJumpPerformed;
        playerInput.Player.Disable();
        moveInput = Vector2.zero;
        jumpQueued = false;
    }

    private void OnDestroy()
    {
        playerInput?.Dispose();
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        UpdateFacingDirection();
        UpdateWaterStateAndMovement();
        HandleJump();
    }

    public void EnterWaterZone(WaterZone zone)
    {
        if (zone != null && !activeWaterZones.Contains(zone))
        {
            activeWaterZones.Add(zone);
        }
    }

    public void ExitWaterZone(WaterZone zone)
    {
        if (zone != null)
        {
            activeWaterZones.Remove(zone);
        }
    }

    public void ResetMovement()
    {
        moveInput = Vector2.zero;
        jumpQueued = false;
        waterJumpGraceTimer = 0f;
        activeWaterZones.Clear();
        IsInWater = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpQueued = true;
    }

    private void UpdateGroundedState()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        IsGrounded = Physics2D.OverlapBox(origin, groundCheckSize, 0f, groundLayer) != null;
    }

    private void UpdateFacingDirection()
    {
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            FacingDirection = moveInput.x > 0f ? 1 : -1;
        }
    }

    private void UpdateWaterStateAndMovement()
    {
        if (waterJumpGraceTimer > 0f)
        {
            waterJumpGraceTimer -= Time.fixedDeltaTime;
            IsInWater = false;
            rb.gravityScale = landGravityScale;
            rb.linearDamping = landDamping;
            rb.linearVelocity = new Vector2(moveInput.x * swimSpeed, rb.linearVelocity.y);
            return;
        }

        if (TryGetMaxSwimY(out float maxSwimY))
        {
            // Let fish sink below the surface before engaging buoyancy so it never stands on water
            if (rb.position.y > maxSwimY + 0.04f)
            {
                IsInWater = false;
                rb.gravityScale = landGravityScale;
                rb.linearDamping = landDamping;
                rb.linearVelocity = new Vector2(moveInput.x * landMoveSpeed, rb.linearVelocity.y);
                return;
            }

            IsInWater = true;
            rb.gravityScale = waterGravityScale;
            rb.linearDamping = waterDamping;

            Vector2 swimVelocity = Vector2.ClampMagnitude(moveInput, 1f) * swimSpeed;
            if (rb.position.y >= maxSwimY && swimVelocity.y > 0f)
            {
                swimVelocity.y = 0f;
                rb.position = new Vector2(rb.position.x, maxSwimY);
            }

            rb.linearVelocity = swimVelocity;
            return;
        }

        IsInWater = false;
        rb.gravityScale = landGravityScale;
        rb.linearDamping = landDamping;
        rb.linearVelocity = new Vector2(moveInput.x * landMoveSpeed, rb.linearVelocity.y);
    }

    private bool TryGetMaxSwimY(out float maxSwimY)
    {
        if (activeWaterZones.Count == 0)
        {
            maxSwimY = 0f;
            return false;
        }

        float highestSurfaceY = float.MinValue;
        for (int i = activeWaterZones.Count - 1; i >= 0; i--)
        {
            if (activeWaterZones[i] == null)
            {
                activeWaterZones.RemoveAt(i);
                continue;
            }

            float surfaceY = activeWaterZones[i].GetWaterSurfaceY(transform.position);
            if (surfaceY > highestSurfaceY)
            {
                highestSurfaceY = surfaceY;
            }
        }

        if (activeWaterZones.Count == 0)
        {
            maxSwimY = 0f;
            return false;
        }

        maxSwimY = highestSurfaceY - surfaceSubmergeDepth;
        return true;
    }

    private void HandleJump()
    {
        if (!jumpQueued)
        {
            return;
        }

        jumpQueued = false;

        if (IsInWater)
        {
            // Keep upward momentum unblocked while leaving the water surface
            waterJumpGraceTimer = dolphinJumpGraceDuration;
            IsInWater = false;
            rb.gravityScale = landGravityScale;
            rb.linearDamping = landDamping;
            rb.linearVelocity = new Vector2(moveInput.x * swimSpeed, dolphinJumpForce);
            OnDolphinJump?.Invoke();
        }
        else if (IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, landJumpForce);
            OnLandJump?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + groundCheckOffset, groundCheckSize);
    }
}
