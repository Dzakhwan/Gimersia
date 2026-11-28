using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class FishVisualController : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private float idleAnimationSpeed = 0.45f;
    [SerializeField] private float activeAnimationSpeed = 1.15f;
    [SerializeField] private float maxSwimTiltAngle = 75f;
    [SerializeField] private float maxAirDownTiltAngle = 45f;
    [SerializeField] private float maxAirUpTiltAngle = 15f;
    [SerializeField] private float rotationSpeed = 720f;

    private static readonly int IsInWaterHash = Animator.StringToHash("IsInWater");

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float currentZAngle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerMove == null)
        {
            playerMove = GetComponentInParent<PlayerMove>();
        }
    }

    private void LateUpdate()
    {
        if (playerMove == null)
        {
            return;
        }

        spriteRenderer.flipX = playerMove.FacingDirection < 0;
        animator.SetBool(IsInWaterHash, playerMove.IsInWater);

        bool isEnergetic = playerMove.IsMoving || (!playerMove.IsInWater && !playerMove.IsGrounded);
        animator.speed = isEnergetic ? activeAnimationSpeed : idleAnimationSpeed;

        UpdateRotation();
    }

    private void UpdateRotation()
    {
        float targetPitch = ComputeTargetPitch();
        float targetZ = targetPitch * playerMove.FacingDirection;
        currentZAngle = Mathf.MoveTowardsAngle(currentZAngle, targetZ, rotationSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentZAngle);
    }

    private float ComputeTargetPitch()
    {
        Vector2 velocity = playerMove.Velocity;

        if (playerMove.IsInWater)
        {
            if (velocity.sqrMagnitude < 0.04f)
            {
                return 0f;
            }

            float rawAngle = Mathf.Atan2(velocity.y, Mathf.Max(0.25f, Mathf.Abs(velocity.x))) * Mathf.Rad2Deg;
            return Mathf.Clamp(rawAngle, -maxSwimTiltAngle, maxSwimTiltAngle);
        }

        if (!playerMove.IsGrounded)
        {
            float airAngle = Mathf.Atan2(velocity.y, Mathf.Max(1.5f, Mathf.Abs(velocity.x))) * Mathf.Rad2Deg;
            return Mathf.Clamp(airAngle, -maxAirDownTiltAngle, maxAirUpTiltAngle);
        }

        return 0f;
    }
}
