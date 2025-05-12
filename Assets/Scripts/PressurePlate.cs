using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("Animator to toggle")]
    public Animator targetAnimator;

    [Header("Name of the Bool parameter to set")]
    public string animatorBoolName = "open";

    [Header("Materials")]
    public Material activeMaterial;
    private Material originalMaterial;

    private GhostMode ghostMode;
    private Renderer plateRenderer;

    private void Awake()
    {
        // Try to find GhostMode on the player via the main camera
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[PressurePlate] Could not find GhostMode on player; ghost check disabled.");

        // Cache the renderer and original material
        plateRenderer = GetComponent<Renderer>();
        if (plateRenderer != null)
            originalMaterial = plateRenderer.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.name}");

        var gm = other.GetComponentInParent<GhostMode>();
        if (gm != null && gm.IsInGhostMode)
        {
            Debug.Log("Ignoring ghosted object.");
            return;
        }

        if (other.attachedRigidbody != null)
        {
            Debug.Log("Object with Rigidbody detected.");
            targetAnimator?.SetBool(animatorBoolName, true);

            // Change to active material
            if (plateRenderer != null && activeMaterial != null)
                plateRenderer.material = activeMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var gm = other.GetComponentInParent<GhostMode>();
        if (gm != null && gm.IsInGhostMode)
            return;

        if (other.attachedRigidbody != null)
        {
            Debug.Log("Object with Rigidbody left.");
            targetAnimator?.SetBool(animatorBoolName, false);

            // Revert to original material
            if (plateRenderer != null && originalMaterial != null)
                plateRenderer.material = originalMaterial;
        }
    }
}
