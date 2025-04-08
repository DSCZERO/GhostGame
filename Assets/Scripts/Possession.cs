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
        playerCamera.transform.localPosition = Vector3.up; 
        playerCamera.transform.localRotation = Quaternion.identity;

        // Enable control on possessed object
        possessedController = target.GetComponent<PossessableController>();
        if (possessedController != null)
            possessedController.SetPossessed(true);

        isPossessing = true;
        currentBody = target;
    }

    void ReturnToPlayer()
    {
        // Make main player body move to new position
        playerBody.transform.position = currentBody.transform.position;

        // Re-enable player control
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Unparent and reset the camera
        playerCamera.transform.SetParent(playerBody.transform);
        playerCamera.transform.localPosition = Vector3.up; // Adjust based on head height
        playerCamera.transform.localRotation = Quaternion.identity;

        // Disable possessed movement
        if (possessedController != null)
            possessedController.SetPossessed(false);

        isPossessing = false;
        currentBody = playerBody;
    }
}