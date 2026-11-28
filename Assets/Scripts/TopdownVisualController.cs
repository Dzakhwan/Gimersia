
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TopdownVisualController : MonoBehaviour
{
    [SerializeField] private TopdownPlayerController player;
    [SerializeField] private float movementThreshold = 0.01f;

    private static readonly int IsMovingTopdown =
        Animator.StringToHash("IsMovingTopdown");

    private Animator animator;
    private Vector2 lastDirection = Vector2.up;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null)
            return;

        Vector2 velocity = player.Velocity;

        bool isMoving =
            velocity.sqrMagnitude >
            movementThreshold * movementThreshold;

        animator.SetBool(IsMovingTopdown, isMoving);

        if (isMoving)
        {
            lastDirection = velocity.normalized;
            UpdateDirection(lastDirection);
        }
    }

    private void UpdateDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, -angle);
    }
}
