using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public static LootSpawner Instance;

    public GameObject lootPrefab;          // The loot prefab (which is what you're spawning)
    public Transform[] spawnPoints;        // The points in the game where loot can spawn
    public float initialSpawnInterval = 5f; // Default interval between loot spawns
    private float currentSpawnInterval;    // The current interval, which can change based on difficulty

    // For visualization of spawn points in the Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;  // Color for the spawn points
        foreach (var spawnPoint in spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint.transform.position, 0.2f);  // Draw a small sphere at the spawn point
        }
    }

    private void Awake()
    {
        // Singleton pattern to ensure only one LootSpawner exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;  // Set the spawn interval
        StartCoroutine(SpawnLoot());  // Start spawning loot
    }

    // Coroutine to spawn loot periodically
    private IEnumerator SpawnLoot()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            SpawnLootItem();  // Spawn a loot item at a random spawn point
        }
    }

    private void SpawnLootItem()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        // Check if there's already loot at the spawn point
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint.position, 0.5f);  // Adjust radius as needed

        // Check for overlap and ensure it is not loot (by checking tag)
        bool isOverlapLoot = false;
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Loot"))  // Check if the collider is tagged as "Loot"
            {
                isOverlapLoot = true;
                break;  // Exit loop early if we find loot overlap
            }
        }

        // If there's no loot overlap, spawn loot
        if (!isOverlapLoot)
        {
            Instantiate(lootPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Loot spawn failed: Overlap detected with loot.");
        }
    }

    // Method to increase loot spawn rate (decrease spawn interval)
    public void IncreaseLootSpawnRate()
    {
        currentSpawnInterval = Mathf.Max(1f, currentSpawnInterval - 1f);  // Never allow spawn interval to go below 1 second
        Debug.Log("Loot spawn rate increased! Current interval: " + currentSpawnInterval);
    }
}
