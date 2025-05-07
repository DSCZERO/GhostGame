using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Possession : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your main camera")]
    public Camera playerCamera;
    [Tooltip("How far you can target for possession")]
    public float interactRange = 5f;
    [Tooltip("The player character GameObject")]
    public GameObject playerBody;
    [Tooltip("Your player's movement script to disable while possessing")]
    public MonoBehaviour playerMovementScript;
    [Tooltip("UI element to show when hovering over a possessable")]
    public GameObject hoverUI;

    [Header("Raycast Mask")]
    [Tooltip("Only these layers will be considered possessable")]
    public LayerMask possessableMask;

    // Reference to GhostMode to allow possession only in ghost form
    private GhostMode ghostMode;

    // Internals
    private GameObject currentBody;
    private PossessableController possessedController;
    private bool isPossessing = false;

    // Player-body components (to hide/disable during possession)
    private Renderer[] playerRenderers;
    private bool[] renderersEnabledState;
    private Collider[] playerColliders;

    // Saved return spot
    private Vector3 savedPosition;

    void Start()
    {
        Debug.Log("[Possession] Start: initializing and caching components");
        currentBody = playerBody;

        // Cache GhostMode from playerCamera parent
        if (playerCamera != null)
        {
            ghostMode = playerCamera.GetComponentInParent<GhostMode>();
            if (ghostMode == null)
                Debug.LogWarning("[Possession] Could not find GhostMode on player; possession restriction disabled.");
            else
                Debug.Log("[Possession] Found GhostMode reference");
        }

        // Cache renderers + their initial enabled state
        playerRenderers = playerBody.GetComponentsInChildren<Renderer>();
        renderersEnabledState = new bool[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
            renderersEnabledState[i] = playerRenderers[i].enabled;
        Debug.Log($"[Possession] Cached {playerRenderers.Length} renderers");

        // Cache colliders
        playerColliders = playerBody.GetComponentsInChildren<Collider>();
        Debug.Log($"[Possession] Cached {playerColliders.Length} colliders");

        // Hide hover UI at start
        if (hoverUI != null)
        {
            hoverUI.SetActive(false);
            Debug.Log("[Possession] Hover UI hidden on Start");
        }

        // Lock cursor initially
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[Possession] Cursor locked and hidden");
    }

    void Update()
    {
        Debug.Log($"[Possession] Update: isPossessing={isPossessing}");
        UpdateHoverUI();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[Possession] E key pressed");
            if (!isPossessing)
            {
                // Only allow possession when in ghost mode
                if (ghostMode != null && !ghostMode.IsInGhostMode)
                {
                    Debug.Log("[Possession] Cannot possess: not in Ghost Mode");
                    return;
                }
                TryPossess();
            }
            else
                ReturnToPlayer();
        }
    }

    // Show/hide hoverUI when pointing at a Possessable
    private void UpdateHoverUI()
    {
        if (hoverUI == null || playerCamera == null)
        {
            Debug.LogWarning("[Possession] UpdateHoverUI: hoverUI or playerCamera reference is missing");
            return;
        }

        // Only show when ghosted and not currently possessing
        if (isPossessing || (ghostMode != null && !ghostMode.IsInGhostMode))
        {
            if (hoverUI.activeSelf)
                Debug.Log("[Possession] Hiding hover UI (not ghost or possessing)");
            hoverUI.SetActive(false);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, possessableMask) &&
            hit.collider.CompareTag("Possessable"))
        {
            if (!hoverUI.activeSelf)
                Debug.Log($"[Possession] Hovering over '{hit.collider.gameObject.name}', showing hover UI");
            hoverUI.SetActive(true);
        }
        else
        {
            if (hoverUI.activeSelf)
                Debug.Log("[Possession] Not hovering over a possessable, hiding hover UI");
            hoverUI.SetActive(false);
        }
    }

    private void TryPossess()
    {
        Debug.Log("[Possession] TryPossess called");
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // Debug: list all encountered colliders
        var allHits = Physics.RaycastAll(ray, interactRange);
        foreach (var h in allHits.OrderBy(h => h.distance))
            Debug.Log($"[Possession][RaycastAll] hit: {h.collider.name} (tag={h.collider.tag}) at d={h.distance:F2}");

        // Only hit Possessable layer
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, possessableMask) &&
            hit.collider.CompareTag("Possessable"))
        {
            savedPosition = playerBody.transform.position;
            Debug.Log($"[Possession] Raycast hit '{hit.collider.gameObject.name}'. Saved player position {savedPosition}");
            Possess(hit.collider.gameObject);
        }
        else
        {
            Debug.Log("[Possession] No possessable object within range (layer filter applied)");
        }
    }

    private void Possess(GameObject target)
    {
        Debug.Log($"[Possession] Starting possession of '{target.name}'");

        // Disable player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            Debug.Log("[Possession] Player movement script disabled");
        }

        // Hide the player's renderers
        foreach (var rend in playerRenderers)
            rend.enabled = false;
        Debug.Log("[Possession] Player renderers hidden");

        // Disable player colliders
        foreach (var col in playerColliders)
            col.enabled = false;
        Debug.Log("[Possession] Player colliders disabled");

        // Parent camera to target and position it
        playerCamera.transform.SetParent(target.transform);
        playerCamera.transform.localPosition = new Vector3(0, 2, -2);
        playerCamera.transform.LookAt(target.transform.position + Vector3.up * 2f);
        Debug.Log("[Possession] Camera parented to target and repositioned");

        // Enable control on possessed object
        possessedController = target.GetComponent<PossessableController>();
        if (possessedController != null)
        {
            possessedController.SetPossessed(true);
            Debug.Log("[Possession] PossessableController.SetPossessed(true) called");
        }

        isPossessing = true;
        currentBody = target;

        // Hide hover UI while possessing
        if (hoverUI != null)
            hoverUI.SetActive(false);

        Debug.Log($"[Possession] Possession complete. Now controlling '{target.name}'");
    }

    private void ReturnToPlayer()
    {
        Debug.Log("[Possession] Returning control to player");

        // Disable movement on the previously possessed object
        if (possessedController != null)
        {
            possessedController.SetPossessed(false);
            Debug.Log("[Possession] PossessableController.SetPossessed(false) called");
        }

        // Teleport player body back to saved position
        playerBody.transform.position = savedPosition;
        Debug.Log($"[Possession] Player body teleported back to {savedPosition}");

        // Re-parent the camera back to the player and reset transform
        playerCamera.transform.SetParent(playerBody.transform);
        playerCamera.transform.localPosition = Vector3.up;
        playerCamera.transform.localRotation = Quaternion.identity;
        Debug.Log("[Possession] Camera reparented to player with default local transform");

        // Re-enable player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
            Debug.Log("[Possession] Player movement script re-enabled");
        }

        // Restore the player's renderer visibility states
        for (int i = 0; i < playerRenderers.Length; i++)
            playerRenderers[i].enabled = renderersEnabledState[i];
        Debug.Log("[Possession] Player renderers visibility restored");

        // Re-enable player colliders
        foreach (var col in playerColliders)
            col.enabled = true;
        Debug.Log("[Possession] Player colliders re-enabled");

        isPossessing = false;
        currentBody = playerBody;

        // Re-lock cursor when returning to player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[Possession] Cursor re-locked and hidden");

        Debug.Log("[Possession] Return to player complete");
    }
}
