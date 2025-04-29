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
        }
    }

    void SendHelpPrompt()
    {
        Debug.Log("AI: Player is struggling. Show help prompt.");
        OnStruggleStatusChanged?.Invoke(true);

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
}
