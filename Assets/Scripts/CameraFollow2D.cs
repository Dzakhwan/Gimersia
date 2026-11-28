using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(7f, 2f);
    [SerializeField] private Vector2 maxBounds = new Vector2(51f, 6f);

    private Vector3 currentVelocity;

    private void Start()
    {
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, GetDesiredPosition(), ref currentVelocity, smoothTime);
    }

    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        transform.position = GetDesiredPosition();
        currentVelocity = Vector3.zero;
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desired = target.position + offset;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }

        desired.z = offset.z;
        return desired;
    }
}
