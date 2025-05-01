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

    private float timer = 0f;
    private float trashCheckInterval = 5f; // Interval for checking if trash is left (in seconds)
    private int trashPicked;
    private bool tracking;
    private float trashCheckTimer = 0f;
    private bool gameEnded = false;
    private float increaseDifficultyCooldown = 0f;
    private float increaseDifficultyInterval = 15f; // e.g., allow IncreaseDifficulty every 30 seconds

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
        if (!tracking || gameEnded) return;

        timer += Time.deltaTime;
        trashCheckTimer += Time.deltaTime;
        increaseDifficultyCooldown += Time.deltaTime;

        if (trashCheckTimer >= trashCheckInterval)
        {
            CheckForTrashLeft();
            trashCheckTimer = 0f;
        }

        if (timer >= timeThreshold)
        {
            if (trashPicked < trashPickupThreshold)
            {
                SendHelpPrompt();
            }
            else
            {
                if (increaseDifficultyCooldown >= increaseDifficultyInterval)
                {
                    IncreaseDifficulty();
                    increaseDifficultyCooldown = 0f;
                }
            }

            StartTracking();
        }
    }

    void StartTracking()
    {
        timer = 0;
        trashPicked = 0;
        tracking = true;
    }

    public void TrackPickup(ItemSO item, int qty)
    {
        Debug.Log($"TrackPickup called with: {item.itemName}, qty: {qty}");

        if (item.itemName.ToLower().Contains("plastic"))
        {
            trashPicked += qty;
            Debug.Log($"Trash picked updated: {trashPicked}");

            // Log trash pickup
            if (logger != null)
                logger.OnTrashPicked();
        }
    }

    public void SendHelpPrompt()
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
        if (gameEnded) return;

        Debug.Log("AI: Player is doing well. Increasing difficulty.");
        OnStruggleStatusChanged?.Invoke(false);

        if (logger != null)
            logger.OnPromptTriggered(PromptType.SpawnMore);

        TargetIndicator indicator = FindObjectOfType<TargetIndicator>();
        if (indicator != null)
            indicator.ClearTarget();

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

    void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("Game Over: No trash left to pick up.");
        Time.timeScale = 0; // Pausing the game

        // End session and label gameplay based on the AI model
        if (logger != null)
        {
            logger.EndSessionAndLabel();
        }

        // You can also trigger a Game Over screen or additional actions here.
        // UIManager.Instance.ShowGameOverScreen(); // optional
    }
}