using UnityEngine;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public List<GameObject> powerUpPrefabs;   // index 0 = Teleport, others = normal
    public int activeCount = 5;               // total power-ups alive at once
    public float spawnY = 6f;

    [Header("Spawn Area (world units)")]
    public float minX = -27f;
    public float maxX =  28f;
    public float minZ = -27f;
    public float maxZ =  28f;

    [Header("Grid")]
    public float cellSize = 5f;               // 5x5 squares
    public float cellPadding = 0.4f;          // shrinks occupancy check so we don’t touch walls
    public bool jitterInsideCell = false;     // if true, randomize within the 5x5 cell a bit
    public float jitterRange = 1.0f;          // +/- range for jitter inside cell (if enabled)

    [Header("Blocked Layers")]
    public LayerMask blockedMask;             // walls/obstacles/etc.

    // ---- internals ----
    private readonly List<GameObject> activePowerUps = new List<GameObject>();
    private readonly Dictionary<GameObject, Vector2Int> puToCell = new Dictionary<GameObject, Vector2Int>();
    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    private int teleportSpawned = 0;
    private int totalSpawned = 0;

    void Start()
    {
        teleportSpawned = 0;
        totalSpawned = 0;

        for (int i = 0; i < activeCount; i++)
            SpawnPowerUp();
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count == 0) return;

        GameObject prefab;

        // First "activeCount" spawns: enforce 2 teleports + the rest random others
        if (totalSpawned < activeCount)
        {
            if (teleportSpawned < 2 && powerUpPrefabs.Count > 0)
            {
                prefab = powerUpPrefabs[0];     // teleport at index 0
                teleportSpawned++;
            }
            else
            {
                prefab = PickRandomOther();
            }
            totalSpawned++;
        }
        else
        {
            // After initial fill, maintain ratio based on what's currently active
            int currentTeleports = 0;
            foreach (var activePU in activePowerUps)
            {
                if (activePU != null && activePU.GetComponent<TeleportPowerUp>() != null)
                    currentTeleports++;
            }
            int currentOthers = Mathf.Max(0, activePowerUps.Count - currentTeleports);

            if (currentTeleports < 2)
                prefab = powerUpPrefabs[0];
            else
                prefab = PickRandomOther();
        }

        // Find a free cell center on the grid
        if (!TryGetFreeCellCenter(out Vector3 center, out Vector2Int cell))
        {
            Debug.LogWarning("PowerUpSpawner: No free grid cells available to spawn.");
            return;
        }

        // Optionally jitter within the 5x5 square
        Vector3 spawnPos = center;
        if (jitterInsideCell && jitterRange > 0f)
        {
            float jx = Random.Range(-jitterRange, jitterRange);
            float jz = Random.Range(-jitterRange, jitterRange);
            spawnPos.x += jx;
            spawnPos.z += jz;
        }

        GameObject spawnedPU = Instantiate(prefab, spawnPos, Quaternion.identity);
        activePowerUps.Add(spawnedPU);

        // Track cell so we can free it later
        puToCell[spawnedPU] = cell;
        occupiedCells.Add(cell);
    }

    GameObject PickRandomOther()
    {
        if (powerUpPrefabs.Count <= 1)
            return powerUpPrefabs[0]; // fallback

        int idx = Random.Range(1, powerUpPrefabs.Count);
        return powerUpPrefabs[idx];
    }

    // Returns a random FREE cell center, avoiding blocked layers and already-used cells
    bool TryGetFreeCellCenter(out Vector3 center, out Vector2Int cellIndex)
    {
        var freeCells = GetAllFreeCells();

        // Remove cells occupied by current powerups
        foreach (var occ in occupiedCells)
            freeCells.Remove(occ);

        if (freeCells.Count == 0)
        {
            center = Vector3.zero;
            cellIndex = default;
            return false;
        }

        cellIndex = freeCells[Random.Range(0, freeCells.Count)];
        center = CellCenter(cellIndex);
        return true;
    }

    // Build a list of grid cells that are empty (no obstacles/props/etc.)
    List<Vector2Int> GetAllFreeCells()
    {
        var cells = new List<Vector2Int>();

        int ixMin = Mathf.CeilToInt(minX / cellSize);
        int ixMax = Mathf.FloorToInt(maxX / cellSize);
        int izMin = Mathf.CeilToInt(minZ / cellSize);
        int izMax = Mathf.FloorToInt(maxZ / cellSize);

        Vector3 half = new Vector3(cellSize * 0.5f - cellPadding, 5f, cellSize * 0.5f - cellPadding);

        for (int ix = ixMin; ix <= ixMax; ix++)
        {
            for (int iz = izMin; iz <= izMax; iz++)
            {
                Vector3 c = CellCenter(new Vector2Int(ix, iz));
                if (Physics.OverlapBox(c + Vector3.up * 1.0f, half, Quaternion.identity, blockedMask).Length == 0)
                {
                    cells.Add(new Vector2Int(ix, iz));
                }
            }
        }

        return cells;
    }

    Vector3 CellCenter(Vector2Int cell)
    {
        float x = cell.x * cellSize;
        float z = cell.y * cellSize;
        return new Vector3(x, spawnY, z);
    }

    // Call this from your power-up scripts when collected
    public void OnPowerUpCollected(GameObject pu)
    {
        if (pu == null) return;

        if (puToCell.TryGetValue(pu, out var cell))
        {
            occupiedCells.Remove(cell);
            puToCell.Remove(pu);
        }

        if (activePowerUps.Contains(pu))
        {
            if (pu.GetComponent<TeleportPowerUp>() != null && totalSpawned <= activeCount)
                teleportSpawned = Mathf.Max(0, teleportSpawned - 1);

            activePowerUps.Remove(pu);
        }

        Destroy(pu);
        SpawnPowerUp(); // keep population constant
    }

    public void OnPowerUpDestroyed(GameObject pu)
    {
        if (pu == null) return;
        if (puToCell.TryGetValue(pu, out var cell))
        {
            occupiedCells.Remove(cell);
            puToCell.Remove(pu);
        }
        activePowerUps.Remove(pu);
        SpawnPowerUp();
    }

    public GameObject GetRandomOtherPowerUp(GameObject exclude)
    {
        List<GameObject> candidates = new List<GameObject>(activePowerUps);
        candidates.RemoveAll(x => x == null || x == exclude);
        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);

        int ixMin = Mathf.CeilToInt(minX / cellSize);
        int ixMax = Mathf.FloorToInt(maxX / cellSize);
        int izMin = Mathf.CeilToInt(minZ / cellSize);
        int izMax = Mathf.FloorToInt(maxZ / cellSize);

        for (int ix = ixMin; ix <= ixMax; ix++)
        {
            for (int iz = izMin; iz <= izMax; iz++)
            {
                Vector3 c = CellCenter(new Vector2Int(ix, iz));
                Gizmos.DrawWireCube(c, new Vector3(cellSize, 0.05f, cellSize));
            }
        }
    }
}
