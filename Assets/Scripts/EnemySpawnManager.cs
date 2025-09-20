using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Level Settings")]
    public bool enableEnemySpawning = true;  // Set to false for Level1, true for Level2
    
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int totalWaves = 5;

    [Header("Spawn Area")]
    public float minX = -31f;  // Reduced from -30 to stay inside brick walls
    public float maxX =  30f;  // Reduced from 31 to stay inside brick walls
    public float minZ = -29f;  // Reduced from -28 to stay inside brick walls
    public float maxZ =  25f;  // Reduced from 24 to stay inside brick walls
    public float spawnY = 0.5f;

    [Header("Collision Check")]
    public LayerMask obstacleMask;   // put your walls/obstacles layer(s) here
    public float checkRadius = 2f;   // >= half enemy width
    public int maxSpawnAttempts = 20;

    [Header("Boundary Enforcement")]
    public bool enableBoundaryEnforcement = true;
    public float boundaryCheckInterval = 0.1f;  // How often to check boundaries (seconds)
    
    [Header("UI")]
    public TMP_Text waveCountText;   // e.g. "Wave 1/5 - Enemies: 6 remaining"
    public TMP_Text winText;         // "YOU WIN!"

    [Header("Level Progression")]
    public string nextLevelScene = "Level2";    // Scene to load after completing
    public float levelCompleteDelay = 3f;      // Delay before loading next level

    [Header("Optional")]
    public bool killOutsideArea = true;

    private int currentWave = 0;
    private bool gameWon = false;
    private Coroutine boundaryCheckCoroutine;

    void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        
        // Start boundary enforcement if enabled
        if (enableBoundaryEnforcement)
        {
            boundaryCheckCoroutine = StartCoroutine(EnforceBoundariesCoroutine());
        }
        
        if (enableEnemySpawning)
        {
            // Level2: Use wave spawning system
            StartCoroutine(RunWaves());
        }
        else
        {
            // Level1: Monitor manually placed enemies
            StartCoroutine(MonitorManualEnemies());
        }
    }

    void Update()
    {
        if (!enableEnemySpawning) 
        {
            // Level1: Show count of manually placed enemies
            int count = CurrentEnemyCount();
            if (waveCountText != null && !gameWon)
            {
                waveCountText.text = $"Enemies remaining: {count}";
            }
            return;
        }

        // Level2: Original wave system logic
        int count2 = CurrentEnemyCount();
        if (waveCountText != null && !gameWon)
        {
            int shownWave = Mathf.Max(currentWave, 1);
            waveCountText.text = $"Wave {shownWave}/{totalWaves} - Enemies: {count2} remaining";
        }
    }

    // Enhanced boundary enforcement using coroutine for better performance
    IEnumerator EnforceBoundariesCoroutine()
    {
        while (!gameWon)
        {
            if (killOutsideArea)
            {
                var enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        Vector3 pos = enemy.transform.position;
                        
                        // Check if enemy is outside boundaries
                        if (pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ)
                        {
                            Debug.Log($"Enemy at {pos} is outside boundaries. Destroying...");
                            Destroy(enemy);
                        }
                        // Optional: Constrain instead of destroy (uncomment if preferred)
                        /*
                        else if (pos.x < minX + 1f || pos.x > maxX - 1f || pos.z < minZ + 1f || pos.z > maxZ - 1f)
                        {
                            // Push enemy back into safe area if they're near the edge
                            Vector3 constrainedPos = new Vector3(
                                Mathf.Clamp(pos.x, minX + 1f, maxX - 1f),
                                pos.y,
                                Mathf.Clamp(pos.z, minZ + 1f, maxZ - 1f)
                            );
                            enemy.transform.position = constrainedPos;
                        }
                        */
                    }
                }
            }
            
            yield return new WaitForSeconds(boundaryCheckInterval);
        }
    }

    // NEW: For Level1 - monitor manually placed enemies
    IEnumerator MonitorManualEnemies()
    {
        // Wait until all manually placed enemies are defeated
        yield return new WaitUntil(() => CurrentEnemyCount() == 0);
        
        // All enemies defeated - complete Level1
        gameWon = true;
        if (winText != null) 
        {
            winText.gameObject.SetActive(true);
            winText.text = "Level 1 Complete!\nLoading Level 2...";
        }
        if (waveCountText != null) waveCountText.text = "All Enemies Defeated!";
        
        Debug.Log("Level1 Complete! Loading Level2...");
        
        // Trigger loading screen + fade at the same time
        FindFirstObjectByType<SceneTransition>().LoadSceneWithTransition(nextLevelScene);
    }

    // Original wave spawning system for Level2
    IEnumerator RunWaves()
    {
        while (currentWave < totalWaves)
        {
            currentWave++;
            Debug.Log($"Starting Wave {currentWave}");

            int toSpawn = Mathf.RoundToInt(
                Mathf.Lerp(6f, 12f, (currentWave - 1f) / (totalWaves - 1f))
            );

            Debug.Log($"Spawning {toSpawn} enemies for wave {currentWave}");

            // Spawn enemies with enhanced validation
            for (int i = 0; i < toSpawn; i++)
            {
                Vector3 spawnPos = FindValidSpawnPosition();
                if (spawnPos != Vector3.zero)
                {
                    GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    
                    // Immediate validation after spawning
                    if (IsPositionOutsideBounds(spawnPos))
                    {
                        Debug.LogError($"ERROR: Enemy spawned outside bounds at {spawnPos}! This should not happen!");
                        Destroy(enemy);
                        continue;
                    }
                    
                    // Add boundary constraint component to enemy if it doesn't exist
                    if (enemy.GetComponent<EnemyBoundaryConstraint>() == null)
                    {
                        var constraint = enemy.AddComponent<EnemyBoundaryConstraint>();
                        constraint.Initialize(minX, maxX, minZ, maxZ);
                    }
                    
                    Debug.Log($"Enemy {i+1} spawned successfully at {spawnPos}");
                }
                else
                {
                    Debug.LogWarning($"Failed to spawn enemy {i+1} - no valid position found");
                }
            }

            yield return new WaitUntil(() => CurrentEnemyCount() == 0);
            Debug.Log($"Wave {currentWave} completed!");
        }

        gameWon = true;
        if (winText != null) 
        {
            winText.gameObject.SetActive(true);
            winText.text = "All Waves Complete!\nYou Win!";
        }
        if (waveCountText != null) waveCountText.text = "Victory!";
        
        Debug.Log("All waves completed!");
        
        // Trigger loading screen + fade at the same time
        FindFirstObjectByType<SceneTransition>().LoadSceneWithTransition(nextLevelScene);
    }

    private void LoadNextLevel()
    {
        // This method is no longer needed since we call transition directly
        // Keep it for backwards compatibility if needed elsewhere
        if (Application.CanStreamedLevelBeLoaded(nextLevelScene))
        {
            Debug.Log($"Loading {nextLevelScene}...");
            SceneManager.LoadScene(nextLevelScene);
        }
        else
        {
            Debug.LogError($"Scene '{nextLevelScene}' not found in Build Settings.");
        }
    }

    int CurrentEnemyCount()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    Vector3 FindValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float x = Random.Range(minX + checkRadius * 2f, maxX - checkRadius * 2f); // Increased buffer
            float z = Random.Range(minZ + checkRadius * 2f, maxZ - checkRadius * 2f); // Increased buffer
            Vector3 candidate = new Vector3(x, spawnY, z);

            // Check for obstacles
            if (Physics.OverlapSphere(candidate, checkRadius, obstacleMask).Length == 0)
            {
                // Double-check bounds
                if (!IsPositionOutsideBounds(candidate))
                {
                    Debug.Log($"Valid spawn position found at {candidate} (attempt {attempt + 1})");
                    return candidate;
                }
            }
        }
        
        Debug.LogWarning($"Failed to find valid spawn position after {maxSpawnAttempts} attempts!");
        return Vector3.zero;
    }

    private bool IsPositionOutsideBounds(Vector3 position)
    {
        return position.x < minX || position.x > maxX || position.z < minZ || position.z > maxZ;
    }

    void OnDrawGizmosSelected()
    {
        // Draw kill area (original boundaries)
        Gizmos.color = Color.red;
        var killCenter = new Vector3((minX + maxX) * 0.5f, spawnY, (minZ + maxZ) * 0.5f);
        var killSize = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(killCenter, killSize);
        
        // Draw safe spawn area (with buffer) - this should be well inside the brick walls
        Gizmos.color = Color.green;
        float bufferX = checkRadius * 2f;
        float bufferZ = checkRadius * 2f;
        var safeSize = new Vector3((maxX - bufferX) - (minX + bufferX), 0.1f, (maxZ - bufferZ) - (minZ + bufferZ));
        var safeCenter = new Vector3((minX + bufferX + maxX - bufferX) * 0.5f, spawnY, (minZ + bufferZ + maxZ - bufferZ) * 0.5f);
        Gizmos.DrawWireCube(safeCenter, safeSize);
        
        // Draw actual brick wall boundaries for reference (approximate)
        Gizmos.color = Color.yellow;
        var brickWallSize = new Vector3(50f, 0.1f, 50f); // Approximate brick wall area
        Gizmos.DrawWireCube(Vector3.zero, brickWallSize);
    }

    void OnDestroy()
    {
        // Stop the boundary check coroutine when the object is destroyed
        if (boundaryCheckCoroutine != null)
        {
            StopCoroutine(boundaryCheckCoroutine);
        }
    }
}

// Additional component to constrain individual enemies
[System.Serializable]
public class EnemyBoundaryConstraint : MonoBehaviour
{
    private float minX, maxX, minZ, maxZ;
    private bool isInitialized = false;

    public void Initialize(float minX, float maxX, float minZ, float maxZ)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minZ = minZ;
        this.maxZ = maxZ;
        this.isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        Vector3 pos = transform.position;
        bool wasConstrained = false;
        
        // Constrain position to boundaries
        if (pos.x < minX)
        {
            pos.x = minX;
            wasConstrained = true;
        }
        else if (pos.x > maxX)
        {
            pos.x = maxX;
            wasConstrained = true;
        }

        if (pos.z < minZ)
        {
            pos.z = minZ;
            wasConstrained = true;
        }
        else if (pos.z > maxZ)
        {
            pos.z = maxZ;
            wasConstrained = true;
        }

        if (wasConstrained)
        {
            transform.position = pos;
            Debug.Log($"Enemy {gameObject.name} was constrained to position {pos}");
        }
    }
}