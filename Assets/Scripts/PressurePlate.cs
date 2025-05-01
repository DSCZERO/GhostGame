using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("Animator to toggle")]
    public Animator targetAnimator;

    [Header("Name of the Bool parameter to set")]
    public string animatorBoolName = "open";

    // Reference to the player's GhostMode (so we can ignore when ghosting)
    private GhostMode ghostMode;

    private void Awake()
    {
        // Try to find GhostMode on the player via the main camera
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[PressurePlate] Could not find GhostMode on player; ghost check disabled.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.name}");

        // 1. If the triggering object (or one of its parents) is in ghost mode, ignore it
        var gm = other.GetComponentInParent<GhostMode>();
        if (gm != null && gm.IsInGhostMode)
        {
            Debug.Log("Ignoring ghosted object.");
            return;
        }

        // 2. Only proceed if this collider belongs to an object with a Rigidbody
        if (other.attachedRigidbody != null)
        {
            Debug.Log("Object with Rigidbody detected.");
            targetAnimator?.SetBool(animatorBoolName, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Same ghost-mode guard on exit
        var gm = other.GetComponentInParent<GhostMode>();
        if (gm != null && gm.IsInGhostMode)
            return;

        if (other.attachedRigidbody != null)
        {
            Debug.Log("Object with Rigidbody left.");
            targetAnimator?.SetBool(animatorBoolName, false);
        }
    }
}
