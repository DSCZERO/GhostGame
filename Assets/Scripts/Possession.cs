using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Possession : MonoBehaviour
{
    public Camera playerCamera;
    public float interactRange = 5f;
    public GameObject playerBody;
    public MonoBehaviour playerMovementScript;

    private GameObject currentBody;
    private PossessableController possessedController;
    private bool isPossessing = false;

    void Start()
    {
        currentBody = playerBody;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!isPossessing)
            {
                TryPossess();
            }
            else
            {
                ReturnToPlayer();
            }
        }
    }

    void TryPossess()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider.CompareTag("Possessable"))
            {
                GameObject target = hit.collider.gameObject;
                Possess(target);
            }
        }
    }

    void Possess(GameObject target)
    {
        // Disable player control
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Parent camera to target
        playerCamera.transform.SetParent(target.transform);
        playerCamera.transform.localPosition = new Vector3(0, 2, -2);
        playerCamera.transform.LookAt(target.transform.position + Vector3.up * 2f);
        //playerCamera.transform.localRotation = Quaternion.identity;

        // Enable control on possessed object
        possessedController = target.GetComponent<PossessableController>();
        if (possessedController != null)
            possessedController.SetPossessed(true);

        isPossessing = true;
        currentBody = target;
    }

    void ReturnToPlayer()
    {
        // Calculate a safe exit position in front of the possessed object
        Vector3 exitOffset = currentBody.transform.forward * 2f;
        Vector3 exitPosition = currentBody.transform.position + exitOffset;

        // Optionally raise the Y position slightly to prevent clipping into the ground
        exitPosition.y += 0.5f;

        // Move player body to the exit position
        playerBody.transform.position = exitPosition;

        // Optionally rotate the player to face the same direction as the possessed object
        playerBody.transform.rotation = Quaternion.LookRotation(currentBody.transform.forward);

        // Re-enable player movement
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Re-parent the camera to the player body and reset its position/rotation
        playerCamera.transform.SetParent(playerBody.transform);
        playerCamera.transform.localPosition = Vector3.up; // Adjust for head height
        playerCamera.transform.localRotation = Quaternion.identity;

        // Disable movement on the previously possessed object
        if (possessedController != null)
            possessedController.SetPossessed(false);

        isPossessing = false;
        currentBody = playerBody;
    }
}