using UnityEngine;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public List<GameObject> powerUpPrefabs; // list of all possible prefabs
    public int activeCount = 3;             // how many to spawn at once
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10);

    private List<GameObject> activePowerUps = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < activeCount; i++)
            SpawnPowerUp();
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs.Count == 0) return;

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Count)];

        // random position within your bounds
        float randX = Random.Range(-29.7f, 30.6f);
        float randZ = Random.Range(-29.6f, 30.6f);

        Vector3 randomPos = new Vector3(randX, 1f, randZ);

        GameObject pu = Instantiate(prefab, randomPos, Quaternion.identity);
        activePowerUps.Add(pu);
    }

    public void OnPowerUpCollected(GameObject pu)
    {
        if (activePowerUps.Contains(pu))
        {
            activePowerUps.Remove(pu);
            Destroy(pu);
            SpawnPowerUp(); // keep count constant
        }
    }

    public GameObject GetRandomOtherPowerUp(GameObject exclude)
    {
        List<GameObject> candidates = new List<GameObject>(activePowerUps);
        candidates.Remove(exclude);
        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];
        return null;
    }
}
