using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    public Transform Target;
    public Transform Player;
    public float Hide;

    void Update()
    {
        if (Target == null || Player == null) return;

        var dir = Target.position - transform.position;

        if (dir.magnitude < Hide)
        {
            SetChildrenActive(false);
        }
        else
        {
            SetChildrenActive(true);
        }

        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void SetChildrenActive(bool value)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(value);
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

    public void Show()
    {
        SetChildrenActive(true);
    }
}
