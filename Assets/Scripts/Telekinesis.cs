using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    [Header("Grab Settings")]
    public float range = 10f;
    public float moveSpeed = 5f;

    [Tooltip("If empty, grabs the first Camera found in children")]
    public Camera playerCamera;

    [Header("UI")]
    [Tooltip("UI element to show when looking at a Telekinesis-tagged object")]
    public GameObject interactUI;

    [Header("Appearance")]
    [Tooltip("Material to apply to the object while grabbed")]
    public Material grabbedMaterial;

    [Header("Disable On Grab")]
    [Tooltip("Any scripts here will be disabled while you’re moving an object")]
    public List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    // (Optional) If you still have Ghost-mode highlighting, keep these:
    public Material ghostModeMaterial;          
    private GhostMode ghostMode;
    private bool wasGhost = false;
    private List<Renderer> telekinesisRenderers = new List<Renderer>();
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private Transform targetedObject;
    private Renderer targetRenderer;
    private Material originalGrabbedMaterial;

    void Start()
    {
        // camera lookup
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                Debug.LogWarning("Telekinesis: No Camera found in children.");
        }

        // ghost-mode (optional)
        if (playerCamera != null)
            ghostMode = playerCamera.GetComponentInParent<GhostMode>();

        // cache all Telekinesis-tagged renderers for ghost highlight
        foreach (var obj in GameObject.FindGameObjectsWithTag("Telekinesis"))
        {
            var rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                telekinesisRenderers.Add(rend);
                originalMaterials[rend] = rend.materials;
            }
        }

        // ensure UI starts hidden
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        // 1) toggle hover UI
        UpdateHoverUI();

        // 2) (optional) ghost-mode material swap
        if (ghostMode != null)
        {
            bool isGhost = ghostMode.IsInGhostMode;
            if (isGhost && !wasGhost) ApplyGhostMaterials();
            else if (!isGhost && wasGhost) RestoreOriginalMaterials();
            wasGhost = isGhost;
        }

        // 3) telekinesis grab/move/release on 'E'
        if (Input.GetKey(KeyCode.E))
        {
            if (targetedObject == null)
            {
                TryFocusOnObject();
                if (targetedObject != null)
                    OnGrab();
            }

            if (targetedObject != null)
                MoveObjectWithInput();
        }
        else if (targetedObject != null)
        {
            ReleaseObject();
        }
    }

    private void UpdateHoverUI()
    {
        if (interactUI == null || playerCamera == null)
            return;

        // raycast from center
        var center = new Vector3(Screen.width / 2f, Screen.height / 2f);
        var ray = playerCamera.ScreenPointToRay(center);
        bool hitTK = Physics.Raycast(ray, out RaycastHit hit, range)
                     && hit.transform.CompareTag("Telekinesis");

        interactUI.SetActive(hitTK);
    }

    private void TryFocusOnObject()
    {
        var screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit, range) &&
            hit.transform.CompareTag("Telekinesis"))
        {
            targetedObject = hit.transform;
        }
    }

    private void OnGrab()
    {
        targetRenderer = targetedObject.GetComponent<Renderer>();
        if (targetRenderer != null && grabbedMaterial != null)
        {
            originalGrabbedMaterial = targetRenderer.material;
            targetRenderer.material = grabbedMaterial;
        }

        foreach (var script in scriptsToDisable)
            if (script != null)
                script.enabled = false;
    }

    private void MoveObjectWithInput()
    {
        Vector3 camF = playerCamera.transform.forward;
        camF.y = 0; camF.Normalize();
        Vector3 camR = playerCamera.transform.right;
        camR.y = 0; camR.Normalize();

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow))      dir += camF;
        if (Input.GetKey(KeyCode.DownArrow))    dir -= camF;
        if (Input.GetKey(KeyCode.RightArrow))   dir += camR;
        if (Input.GetKey(KeyCode.LeftArrow))    dir -= camR;
        if (Input.GetKey(KeyCode.RightShift))   dir += Vector3.up;
        if (Input.GetKey(KeyCode.RightControl)) dir -= Vector3.down;

        if (dir != Vector3.zero)
            targetedObject.position += dir * moveSpeed * Time.deltaTime;
    }

    private void ReleaseObject()
    {
        if (targetRenderer != null && originalGrabbedMaterial != null)
            targetRenderer.material = originalGrabbedMaterial;

        foreach (var script in scriptsToDisable)
            if (script != null)
                script.enabled = true;

        targetedObject = null;
        targetRenderer  = null;
    }

    // Ghost-mode helpers (optional)
    private void ApplyGhostMaterials()
    {
        if (ghostModeMaterial == null) return;
        foreach (var rend in telekinesisRenderers)
        {
            if (rend == null) continue;
            var mats = new Material[rend.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = ghostModeMaterial;
            rend.materials = mats;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
                kvp.Key.materials = kvp.Value;
        }
    }
}
