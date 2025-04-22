using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Telekenisis : MonoBehaviour
{
    public float range = 10f;
    public float moveSpeed = 5f;
    public Camera playerCamera;
    public FirstPersonController playerController;
    public MonoBehaviour playerMovementScript;


    private Transform targetedObject;

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            if (targetedObject == null)
                TryFocusOnObject();

            if (targetedObject != null)
            {
                MoveObjectWithInput();

                if (playerController.enabled)
                { 
                    playerController.enabled = false; 
                }

                // Disable player control
                if (playerMovementScript != null)
                    playerMovementScript.enabled = false;
            }
        }
        else
        {
            if (targetedObject != null)
            {
                targetedObject = null; // Release object when Q is released
                playerController.enabled = true; // Re-enable on release
                // Disable player control
                if (playerMovementScript != null)
                    playerMovementScript.enabled = false;
            }
        }
    }

    void TryFocusOnObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.transform.CompareTag("Telekinesis"))
            {
                targetedObject = hit.transform;
            }
        }
    }

    void MoveObjectWithInput()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow)) direction += Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow)) direction += Vector3.right;

        targetedObject.position += direction * moveSpeed * Time.deltaTime;
    }
}
