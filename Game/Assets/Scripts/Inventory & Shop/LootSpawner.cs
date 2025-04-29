using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public static LootSpawner Instance;

    [Header("Loot Settings")]
    public List<GameObject> lootPrefabs;        // List of loot prefabs
    public Transform[] spawnPoints;             // Points where loot can spawn

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
                Gizmos.DrawSphere(spawnPoint.transform.position, 0.2f);
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SpawnLootItem()
    {
        if (lootPrefabs == null || lootPrefabs.Count == 0)
        {
            Debug.LogError("LootSpawner: No loot prefabs available! Cannot spawn loot.");
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("LootSpawner: No spawn points assigned!");
            return;
        }

        // Pick a random loot prefab from the list
        GameObject selectedLootPrefab = lootPrefabs[Random.Range(0, lootPrefabs.Count)];

        if (selectedLootPrefab == null)
        {
            Debug.LogError("LootSpawner: Selected loot prefab is null!");
            return;
        }

        // Pick a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Check if something is already there
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint.position, 0.5f);

        bool isOverlapLoot = false;
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Loot"))
            {
                isOverlapLoot = true;
                break;
            }
        }

        if (!isOverlapLoot)
        {
            Instantiate(selectedLootPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("LootSpawner: Spawned " + selectedLootPrefab.name + " at " + spawnPoint.position);
        }
        else
        {
            Debug.LogWarning("LootSpawner: Overlap detected at spawn point. Skipping spawn.");
        }
    }
}