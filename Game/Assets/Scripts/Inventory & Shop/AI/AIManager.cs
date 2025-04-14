using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    public float timeThreshold = 30f; // Time to consider as struggling
    public int trashPickupThreshold = 5; // Considered successful if player picks this many within threshold

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
        // You can add UI prompts to assist the player
    }

    void IncreaseDifficulty()
    {
        Debug.Log("AI: Player is doing well. Increasing difficulty.");
    }
}
