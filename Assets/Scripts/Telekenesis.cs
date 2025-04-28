using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    [Header("Grab Settings")]
    public float range = 10f;
    public float moveSpeed = 5f;

    [Tooltip("If empty, grabs the first Camera found in children")]
    public Camera playerCamera;

    [Header("Appearance")]
    [Tooltip("Material to apply to the object while grabbed")]
    public Material grabbedMaterial;

    [Header("Disable On Grab")]
    [Tooltip("Any scripts here will be disabled while you’re moving an object")]
    public List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    private Transform targetedObject;
    private Renderer targetRenderer;
    private Material originalMaterial;

    void Start()
    {
        // auto‑find child camera if none assigned
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                Debug.LogWarning("Telekinesis: No Camera found in children.");
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            // on first press, try to lock on
            if (targetedObject == null)
            {
                TryFocusOnObject();
                if (targetedObject != null)
                    OnGrab();
            }

            // if we have a target, move it
            if (targetedObject != null)
                MoveObjectWithInput();
        }
        else
        {
            // on release, put everything back
            if (targetedObject != null)
                ReleaseObject();
        }
    }

    void TryFocusOnObject()
    {
        var screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit, range) &&
            hit.transform.CompareTag("Telekinesis"))
        {
            targetedObject = hit.transform;
        }
    }

    void OnGrab()
    {
        // swap material
        targetRenderer = targetedObject.GetComponent<Renderer>();
        if (targetRenderer != null && grabbedMaterial != null)
        {
            originalMaterial = targetRenderer.material;
            targetRenderer.material = grabbedMaterial;
        }

        // disable listed scripts
        foreach (var script in scriptsToDisable)
            if (script != null)
                script.enabled = false;
    }

    void MoveObjectWithInput()
    {
        // Get the camera’s forward and right vectors, flattened to the XZ plane
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // Build your movement direction based on arrow keys
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow)) dir += camForward;
        if (Input.GetKey(KeyCode.DownArrow)) dir -= camForward;
        if (Input.GetKey(KeyCode.RightArrow)) dir += camRight;
        if (Input.GetKey(KeyCode.LeftArrow)) dir -= camRight;

        // Apply movement
        if (dir != Vector3.zero)
            targetedObject.position += dir * moveSpeed * Time.deltaTime;
    }


    void ReleaseObject()
    {
        // restore material
        if (targetRenderer != null && originalMaterial != null)
            targetRenderer.material = originalMaterial;

        // re‑enable scripts
        foreach (var script in scriptsToDisable)
            if (script != null)
                script.enabled = true;

        // clear references
        targetedObject = null;
        targetRenderer = null;
    }
}
