using System;
using System.Collections;
using UnityEngine;

public class PlayerPerformanceSimulator : MonoBehaviour
{
    public enum PerformanceType
    {
        Good,
        Medium,
        Poor
    }

    public PerformanceType performanceType = PerformanceType.Good;
    private AIManager aiManager;
    private float moveSpeed = 3f; // Speed at which the player moves
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isCollidingWithWall = false;  // To check if player is colliding with a wall

    private float timeSinceLastLootPickup = 0f; // Tracks time since last loot pickup (for poor performance)
    private float helpHintThreshold = 5f; // Time threshold to trigger help hint for poor performance

    void Start()
    {
        aiManager = AIManager.Instance;
        SimulatePlayerPerformance();
    }

    void SimulatePlayerPerformance()
    {
        switch (performanceType)
        {
            case PerformanceType.Good:
                StartCoroutine(SimulateGoodPlayer());
                break;
            case PerformanceType.Medium:
                StartCoroutine(SimulateMediumPlayer());
                break;
            case PerformanceType.Poor:
                StartCoroutine(SimulatePoorPlayer());
                break;
        }
    }

    // Simulate good player behavior
    private IEnumerator SimulateGoodPlayer()
    {
        while (true)
        {
            if (!isMoving)
            {
                // Find a random loot item
                Loot targetLoot = FindRandomLoot();
                if (targetLoot != null)
                {
                    targetPosition = targetLoot.transform.position;
                    isMoving = true;
                    StartCoroutine(MoveToTarget(targetLoot.transform));
                }
            }

            // Simulate fast trash pickup (e.g., pickup trash every 1 second)
            aiManager.TrackPickup(new ItemSO { itemName = "Plastic" }, 1);
            yield return new WaitForSeconds(6f); // Player collects trash every second
        }
    }

    private IEnumerator SimulateMediumPlayer()
    {
        while (true)
        {
            if (!isMoving)
            {
                Loot targetLoot = FindRandomLoot();
                if (targetLoot != null)
                {
                    targetPosition = targetLoot.transform.position;
                    isMoving = true;
                    StartCoroutine(MoveToTarget(targetLoot.transform));
                }
            }

            // Chance-based help prompt
            if (UnityEngine.Random.value < 0.3f)
            {
                aiManager.SendHelpPrompt();
            }

            // Chance-based spawn more
            if (UnityEngine.Random.value < 0.2f)
            {
                aiManager.IncreaseDifficulty();
            }

            aiManager.TrackPickup(new ItemSO { itemName = "Plastic" }, 1);

            float waitTime = UnityEngine.Random.Range(4f, 6f);
            yield return new WaitForSeconds(waitTime); // Moderate speed trash pickup
        }
    }

    // Simulate poor player behavior
    private IEnumerator SimulatePoorPlayer()
    {
        while (true)
        {
            if (!isMoving)
            {
                // Find a random loot item
                Loot targetLoot = FindRandomLoot();
                if (targetLoot != null)
                {
                    targetPosition = targetLoot.transform.position;
                    isMoving = true;
                    StartCoroutine(MoveToTarget(targetLoot.transform));
                }
            }

            // Simulate slower trash pickup (e.g., pickup trash every 3 seconds)
            timeSinceLastLootPickup += Time.deltaTime; // Track the time since last loot pickup

            if (timeSinceLastLootPickup >= helpHintThreshold)
            {
                // Trigger help hint if the player is struggling
                aiManager.SendHelpPrompt();
                timeSinceLastLootPickup = 0f; // Reset the timer after showing a hint
            }

            aiManager.TrackPickup(new ItemSO { itemName = "Plastic" }, 1);
            yield return new WaitForSeconds(10f); // Player collects trash every 3 seconds
        }
    }

    private Loot FindRandomLoot()
    {
        // Get all loot items in the scene
        Loot[] allLoots = FindObjectsOfType<Loot>();
        if (allLoots.Length == 0)
        {
            return null;
        }

        // Randomly select a loot item
        return allLoots[UnityEngine.Random.Range(0, allLoots.Length)];
    }

    private IEnumerator MoveToTarget(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            // If colliding with a wall, change direction
            if (isCollidingWithWall)
            {
                ChangeDirection();
                isCollidingWithWall = false;
            }

            // Move the player towards the target position
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Once reached, the player "picks up" the loot and we stop moving
        isMoving = false;
    }

    private void ChangeDirection()
    {
        // Change direction randomly (mimicking the flip action)
        Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
        targetPosition = transform.position + randomDirection * 5f; // 5 units away from current position
    }

    // Detect collision with walls
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isCollidingWithWall = true;
        }
    }
}