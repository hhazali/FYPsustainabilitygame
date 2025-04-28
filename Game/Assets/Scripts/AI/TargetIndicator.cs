using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public Transform Target;         // The loot/item to point toward
    public Transform Player;         // The player to follow
    public float HeightAbovePlayer = 2f;
    public float Hide = 1.5f;        // Hide indicator when close to target

    void LateUpdate()
    {
        if (Target == null || Player == null)
        {
            SetChildrenActive(false);
            return;
        }

        // Position above the player
        transform.position = Player.position + Vector3.up * HeightAbovePlayer;

        // Direction from indicator to target
        Vector2 dir = Target.position - transform.position;

        // Show or hide indicator based on distance
        if (dir.magnitude < Hide)
        {
            SetChildrenActive(false);
        }
        else
        {
            SetChildrenActive(true);

            // Rotate to point toward the target
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        Target = newTarget;
        SetChildrenActive(true);
    }

    public void ClearTarget()
    {
        Target = null;
        SetChildrenActive(false);
    }

    void SetChildrenActive(bool value)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(value);
        }
    }
}
