using System.Collections.Generic;
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
        currentBody = playerBody;

        // Cache renderers + their initial enabled state
        playerRenderers = playerBody.GetComponentsInChildren<Renderer>();
        renderersEnabledState = new bool[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
            renderersEnabledState[i] = playerRenderers[i].enabled;

        // Cache colliders
        playerColliders = playerBody.GetComponentsInChildren<Collider>();

        // Hide hover UI at start
        if (hoverUI != null)
            hoverUI.SetActive(false);

        // Lock cursor initially
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateHoverUI();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPossessing)
                TryPossess();
            else
                ReturnToPlayer();
        }
    }

    // Show/hide hoverUI when pointing at a Possessable (only when not currently possessing)
    private void UpdateHoverUI()
    {
        if (hoverUI == null || playerCamera == null)
            return;

        if (isPossessing)
        {
            hoverUI.SetActive(false);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) &&
            hit.collider.CompareTag("Possessable"))
        {
            hoverUI.SetActive(true);
        }
        else
        {
            hoverUI.SetActive(false);
        }
    }

    private void TryPossess()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) &&
            hit.collider.CompareTag("Possessable"))
        {
            // Save current player-body position
            savedPosition = playerBody.transform.position;
            Possess(hit.collider.gameObject);
        }
    }

    private void Possess(GameObject target)
    {
        // Disable player movement
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Hide the player's renderers
        for (int i = 0; i < playerRenderers.Length; i++)
            playerRenderers[i].enabled = false;

        // Disable player colliders
        foreach (var col in playerColliders)
            col.enabled = false;

        // Parent camera to target and position it
        playerCamera.transform.SetParent(target.transform);
        playerCamera.transform.localPosition = new Vector3(0, 2, -2);
        playerCamera.transform.LookAt(target.transform.position + Vector3.up * 2f);

        // Enable control on possessed object
        possessedController = target.GetComponent<PossessableController>();
        if (possessedController != null)
            possessedController.SetPossessed(true);

        isPossessing = true;
        currentBody = target;

        // Hide hover UI while possessing
        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    private void ReturnToPlayer()
    {
        // Disable movement on the previously possessed object
        if (possessedController != null)
            possessedController.SetPossessed(false);

        // Teleport player body back to saved position
        playerBody.transform.position = savedPosition;

        // Re-parent the camera to the player body and reset its transform
        playerCamera.transform.SetParent(playerBody.transform);
        playerCamera.transform.localPosition = Vector3.up;   // Adjust for head height
        playerCamera.transform.localRotation = Quaternion.identity;

        // Re-enable player movement
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Restore the player's renderer visibility states
        for (int i = 0; i < playerRenderers.Length; i++)
            playerRenderers[i].enabled = renderersEnabledState[i];

        // Re-enable player colliders
        foreach (var col in playerColliders)
            col.enabled = true;

        isPossessing = false;
        currentBody = playerBody;

        // Re-lock cursor when returning to player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}