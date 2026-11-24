using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterZone : MonoBehaviour
{
    [SerializeField] private bool setsCheckpoint = true;
    [SerializeField] private Transform customRespawnPoint;

    private Tilemap tilemap;
    private Collider2D zoneCollider;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        zoneCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.EnterWaterZone(this);
        }

        if (setsCheckpoint && other.TryGetComponent(out PlayerHealth health))
        {
            health.SetCheckpoint(ResolveCheckpointPosition(other.transform.position));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.ExitWaterZone(this);
        }
    }

    public float GetWaterSurfaceY(Vector3 worldPosition)
    {
        if (tilemap != null && TryFindWaterCell(worldPosition, out Vector3Int cell))
        {
            while (tilemap.HasTile(cell + Vector3Int.up))
            {
                cell += Vector3Int.up;
            }

            return tilemap.CellToWorld(cell).y + tilemap.cellSize.y;
        }

        return zoneCollider != null ? zoneCollider.bounds.max.y : transform.position.y;
    }

    private Vector3 ResolveCheckpointPosition(Vector3 contactPosition)
    {
        if (customRespawnPoint != null)
        {
            return customRespawnPoint.position;
        }

        if (tilemap != null && TryFindWaterCell(contactPosition, out Vector3Int cell))
        {
            return tilemap.GetCellCenterWorld(cell);
        }

        return zoneCollider != null ? zoneCollider.bounds.center : transform.position;
    }

    private bool TryFindWaterCell(Vector3 worldPosition, out Vector3Int foundCell)
    {
        Vector3Int originCell = tilemap.WorldToCell(worldPosition);
        Vector3Int[] offsets =
        {
            Vector3Int.zero,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.up
        };

        foreach (Vector3Int offset in offsets)
        {
            Vector3Int candidate = originCell + offset;
            if (tilemap.HasTile(candidate))
            {
                foundCell = candidate;
                return true;
            }
        }

        foundCell = originCell;
        return false;
    }
}
