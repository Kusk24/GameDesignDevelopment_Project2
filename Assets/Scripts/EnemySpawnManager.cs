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
    public float minX = -30f;
    public float maxX =  31f;
    public float minZ = -28f;
    public float maxZ =  24f;
    public float spawnY = 0.5f;

    [Header("Collision Check")]
    public LayerMask obstacleMask;   // put your walls/obstacles layer(s) here
    public float checkRadius = 2f;   // >= half enemy width
    public int maxSpawnAttempts = 20;

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

    void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        
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

        if (killOutsideArea && count2 > 0)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                Vector3 p = e.transform.position;
                if (p.x < minX  || p.x > maxX  || p.z < minZ  || p.z > maxZ )
                    Destroy(e);
            }
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
        
        // Wait a bit then load Level2
        yield return new WaitForSeconds(levelCompleteDelay);
        LoadNextLevel();
    }

    // Original wave spawning system for Level2
    IEnumerator RunWaves()
    {
        while (currentWave < totalWaves)
        {
            currentWave++;

            int toSpawn = Mathf.RoundToInt(
                Mathf.Lerp(6f, 12f, (currentWave - 1f) / (totalWaves - 1f))
            );

            for (int i = 0; i < toSpawn; i++)
            {
                Vector3 pos = FindValidSpawnPosition();
                if (pos != Vector3.zero)
                    Instantiate(enemyPrefab, pos, Quaternion.identity);
            }

            yield return new WaitUntil(() => CurrentEnemyCount() == 0);
        }

        gameWon = true;
        if (winText != null) 
        {
            winText.gameObject.SetActive(true);
            winText.text = "All Waves Complete!\nYou Win!";
        }
        if (waveCountText != null) waveCountText.text = "Victory!";
        
        // Level2 complete - load next scene or return to menu
        yield return new WaitForSeconds(levelCompleteDelay);
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
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
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            Vector3 candidate = new Vector3(x, spawnY, z);

            if (Physics.OverlapSphere(candidate, checkRadius, obstacleMask).Length == 0)
                return candidate;
        }
        return Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var center = new Vector3((minX + maxX) * 0.5f, spawnY, (minZ + maxZ) * 0.5f);
        var size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }
}
