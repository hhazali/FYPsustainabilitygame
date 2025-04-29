using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    public float timeThreshold = 30f; // Time to consider as struggling
    public int trashPickupThreshold = 5; // Considered successful if player picks this many within threshold
    public event Action<bool> OnStruggleStatusChanged;

    private float timer;
    private int trashPicked;
    private bool tracking;

    private GameplayLogger logger;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        Loot.OnItemLooted += TrackPickup;
    }

    private void OnDisable()
    {
        Loot.OnItemLooted -= TrackPickup;
    }

    private void Start()
    {
        StartTracking();

        // Cache the logger instance if available
        logger = FindObjectOfType<GameplayLogger>();
        if (logger == null)
            Debug.LogWarning("AIManager: No GameplayLogger found in the scene.");
    }

    void Update()
    {
        if (!tracking) return;

        timer += Time.deltaTime;

        if (timer >= timeThreshold)
        {
            if (trashPicked < trashPickupThreshold)
            {
                SendHelpPrompt(); // If player is struggling
            }
            else
            {
                IncreaseDifficulty(); // If player is efficient
            }

            // Check if no trash is left in the game
            CheckForTrashLeft();

            // Reset for next cycle
            StartTracking();
        }
    }

    void StartTracking()
    {
        timer = 0;
        trashPicked = 0;
        tracking = true;
    }

    void TrackPickup(ItemSO item, int qty)
    {
        Debug.Log($"TrackPickup called with: {item.itemName}, qty: {qty}");

        if (item.itemName.ToLower().Contains("plastic"))
        {
            trashPicked += qty;
            Debug.Log($"Trash picked updated: {trashPicked}");

            // ✨ Log trash pickup
            if (logger != null)
                logger.OnTrashPicked();
        }
    }

    void SendHelpPrompt()
    {
        Debug.Log("AI: Player is struggling. Show help prompt.");
        OnStruggleStatusChanged?.Invoke(true);

        // Log prompt triggered
        if (logger != null)
            logger.OnPromptTriggered(PromptType.HelpHint);  // Pass the HelpHint enum

        // Find random plastic loot to point at
        TargetIndicator indicator = FindObjectOfType<TargetIndicator>();
        if (indicator == null)
        {
            Debug.LogWarning("AI: No TargetIndicator found!");
            return;
        }

        List<Loot> plasticLoots = new List<Loot>();

        foreach (Loot loot in FindObjectsOfType<Loot>())
        {
            if (loot != null && loot.itemSO.itemName.ToLower().Contains("plastic"))
            {
                plasticLoots.Add(loot);
            }
        }

        if (plasticLoots.Count == 0)
        {
            Debug.Log("AI: No trash left.");
            indicator.ClearTarget();
            return;
        }

        // Pick a random plastic loot
        Loot chosenLoot = plasticLoots[UnityEngine.Random.Range(0, plasticLoots.Count)];
        indicator.SetTarget(chosenLoot.transform);
    }

    void IncreaseDifficulty()
    {
        Debug.Log("AI: Player is doing well. Increasing difficulty.");
        OnStruggleStatusChanged?.Invoke(false);

        if (logger != null)
        logger.OnPromptTriggered(PromptType.SpawnMore);

        // Clear any help target
        TargetIndicator indicator = FindObjectOfType<TargetIndicator>();
        if (indicator != null)
        {
            indicator.ClearTarget();
        }

        // Tell LootSpawner to spawn new loot manually
        if (LootSpawner.Instance != null)
        {
            LootSpawner.Instance.SpawnLootItem();
        }
        else
        {
            Debug.LogWarning("AIManager: No LootSpawner found.");
        }
    }

    // New method to check if no trash is left
    void CheckForTrashLeft()
    {
        List<Loot> plasticLoots = new List<Loot>();

        foreach (Loot loot in FindObjectsOfType<Loot>())
        {
            if (loot != null && loot.itemSO.itemName.ToLower().Contains("plastic"))
            {
                plasticLoots.Add(loot);
            }
        }

        // If no plastic trash is left, end the game
        if (plasticLoots.Count == 0)
        {
            Debug.Log("AI: No trash left.");
            EndGame();
        }
    }

    // New method to end the game
    void EndGame()
    {
        Debug.Log("Game Over: No trash left to pick up.");

        // Optionally pause the game
        Time.timeScale = 0; // Freezes the game

        // Trigger any UI or additional actions to show the game-over message
        // Example: Display a "Game Over" message
        // UIManager.Instance.ShowGameOverScreen();
    }
}