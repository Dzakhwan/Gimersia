using UnityEngine;
using UnityEngine.SceneManagement;

// Pintu penghubung antar scene. Saat ikan menyentuh trigger ini, scene tujuan dimuat
// dan ikan muncul di depan pintu yang id-nya sama dengan targetDoorId.
[RequireComponent(typeof(Collider2D))]
public class SceneDoor : MonoBehaviour
{
    [SerializeField] private string doorId = "LeftDoor";
    [SerializeField] private string targetScene = "";
    [SerializeField] private string targetDoorId = "";
    [Tooltip("Posisi muncul relatif terhadap pintu ini saat datang dari scene lain.")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(1.5f, 0f);
    [SerializeField] private float armDelay = 0.4f;

    private static string pendingDoorId;
    private static bool isLoading;

    private float armedAt;

    public string DoorId => doorId;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        armedAt = Time.time + armDelay;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(pendingDoorId) || pendingDoorId != doorId)
        {
            return;
        }

        pendingDoorId = null;
        isLoading = false;
        PlacePlayerAtSpawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || Time.time < armedAt)
        {
            return;
        }

        if (!other.TryGetComponent(out PlayerMove _))
        {
            return;
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning($"SceneDoor '{doorId}' di {gameObject.scene.name} belum punya targetScene.");
            return;
        }

        isLoading = true;
        pendingDoorId = targetDoorId;
        Time.timeScale = 1f;
        SceneManager.LoadScene(targetScene);
    }

    private void PlacePlayerAtSpawn()
    {
        PlayerMove player = FindFirstObjectByType<PlayerMove>();
        if (player == null)
        {
            return;
        }

        Vector3 spawn = transform.position + (Vector3)spawnOffset;
        spawn.z = player.transform.position.z;

        player.ResetMovement();
        player.transform.position = spawn;
        if (player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.position = spawn;
        }

        if (player.TryGetComponent(out PlayerHealth health))
        {
            health.SetCheckpoint(spawn);
        }

        CameraFollow2D cam = FindFirstObjectByType<CameraFollow2D>();
        if (cam != null)
        {
            cam.SnapToTarget();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 spawn = transform.position + (Vector3)spawnOffset;
        Gizmos.DrawWireSphere(spawn, 0.3f);
        Gizmos.DrawLine(transform.position, spawn);
    }
}
