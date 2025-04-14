using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public Transform target; // Loot
    public Transform player; // Player

    public float heightAbovePlayer = 2f;

    void Update()
    {
        if (target == null || player == null)
        {
            gameObject.SetActive(false); // Hide if nothing to point to
            return;
        }

        // Position above the player
        transform.position = player.position + Vector3.up * heightAbovePlayer;

        // Point toward the target loot
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetTarget(Transform newTarget)
    {
        Debug.Log("TargetIndicator: Set target to " + newTarget.name);
        target = newTarget;
        gameObject.SetActive(true);
    }

    public void ClearTarget()
    {
        target = null;
        gameObject.SetActive(false);
    }
}
