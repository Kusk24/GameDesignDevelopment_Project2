using UnityEngine;
using System.Collections;
using TMPro;

public class EnemySpawnManager : MonoBehaviour
{
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

    [Header("Optional")]
    public bool killOutsideArea = true; // culls escapees using same bounds

    private int currentWave = 0;
    private bool gameWon = false;

    void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        StartCoroutine(RunWaves());
    }

    void Update()
    {
        // live enemy count by tag (no other scripts needed)
        int count = CurrentEnemyCount();

        if (waveCountText != null && !gameWon)
        {
            int shownWave = Mathf.Max(currentWave, 1);
            waveCountText.text = $"Wave {shownWave}/{totalWaves} - Enemies: {count} remaining";
        }

        if (killOutsideArea && count > 0)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                Vector3 p = e.transform.position;
                if (p.x < minX -3 || p.x > maxX +3|| p.z < minZ-3 || p.z > maxZ+3)
                    Destroy(e);
            }
        }
    }

    IEnumerator RunWaves()
    {
        while (currentWave < totalWaves)
        {
            currentWave++;

            // enemies per wave: 6 → 12 across waves
            int toSpawn = Mathf.RoundToInt(
                Mathf.Lerp(6f, 12f, (currentWave - 1f) / (totalWaves - 1f))
            );

            // spawn
            for (int i = 0; i < toSpawn; i++)
            {
                Vector3 pos = FindValidSpawnPosition();
                if (pos != Vector3.zero)
                    Instantiate(enemyPrefab, pos, Quaternion.identity);
            }

            // wait until all enemies (by tag) are gone
            yield return new WaitUntil(() => CurrentEnemyCount() == 0);
        }

        // done
        gameWon = true;
        if (winText != null) winText.gameObject.SetActive(true);
        if (waveCountText != null) waveCountText.text = "All Waves Complete!";
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

            // keep clear of walls/obstacles
            if (Physics.OverlapSphere(candidate, checkRadius, obstacleMask).Length == 0)
                return candidate;
        }
        return Vector3.zero; // give up this slot
    }

    // visualize spawn rectangle in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var center = new Vector3((minX + maxX) * 0.5f, spawnY, (minZ + maxZ) * 0.5f);
        var size   = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }
}
