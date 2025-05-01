using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pyrokinesis : MonoBehaviour
{
    // Tracks whether the player has "heat" stored.
    private bool heat = false;

    // Maximum distance for the raycast to check for objects.
    public float rayDistance = 5f;

    [Header("Audio")]
    public AudioSource audioSource;    
    public AudioClip storeHeatClip;    
    public AudioClip useHeatClip;      

    [Header("UI")]
    public RawImage heatIndicator;     
    [Tooltip("UI element to show when looking at a Fire or Flammable object")]
    public GameObject interactUI;

    [Header("Ghost Highlight")]
    [Tooltip("Material to apply to flammable objects while in Ghost Mode")]
    public Material flammableGhostMaterial;

    private GhostMode ghostMode;
    private bool isHighlightActive = false;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private int flammableLayer;

    void Awake()
    {
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[Pyrokinesis] GhostMode not found; pyrokinesis disabled.");

        flammableLayer = LayerMask.NameToLayer("Flammable");
        if (flammableLayer < 0)
            Debug.LogError("[Pyrokinesis] Layer \"Flammable\" missing—add it in Tags & Layers.");
    }

    void Start()
    {
        if (heatIndicator != null)
            heatIndicator.color = Color.black;

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        // Always update UI prompt, but only show it when ghosted
        UpdateInteractUI();

        if (ghostMode == null)
            return;

        // Handle flammable highlight on ghost-enter/exit
        bool nowGhosted = ghostMode.IsInGhostMode;
        if (nowGhosted && !isHighlightActive)
            ActivateFlammableHighlight();
        else if (!nowGhosted && isHighlightActive)
            RevertFlammableHighlight();

        // Only allow interactions when ghosted
        if (!nowGhosted)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
            );

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                // Pick up heat from Fire-tagged object (and destroy it)
                if (hit.transform.CompareTag("Fire") && !heat)
                {
                    var rend = hit.transform.GetComponent<Renderer>();
                    if (rend != null) rend.material.color = Color.white;

                    heat = true;
                    audioSource?.PlayOneShot(storeHeatClip);
                    heatIndicator.color = Color.white;

                    Destroy(hit.transform.gameObject);
                }
                // Consume heat on Flammable-layer object
                else if (heat && hit.transform.gameObject.layer == flammableLayer)
                {
                    Destroy(hit.transform.gameObject);
                    heat = false;
                    audioSource?.PlayOneShot(useHeatClip);
                    heatIndicator.color = Color.black;
                }
            }
        }
    }

    private void UpdateInteractUI()
    {
        if (interactUI == null || Camera.main == null || ghostMode == null)
            return;

        // Only show UI when in ghost mode
        if (!ghostMode.IsInGhostMode)
        {
            interactUI.SetActive(false);
            return;
        }

        Ray focusRay = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        bool showPrompt = false;
        if (Physics.Raycast(focusRay, out RaycastHit hit, rayDistance))
        {
            if (hit.transform.CompareTag("Fire") ||
                hit.transform.gameObject.layer == flammableLayer)
            {
                showPrompt = true;
            }
        }

        interactUI.SetActive(showPrompt);
    }

    private void ActivateFlammableHighlight()
    {
        foreach (var rend in FindObjectsOfType<Renderer>())
        {
            if (rend.gameObject.layer == flammableLayer)
            {
                originalMaterials[rend] = rend.materials;
                var highlights = new Material[rend.materials.Length];
                for (int i = 0; i < highlights.Length; i++)
                    highlights[i] = flammableGhostMaterial;
                rend.materials = highlights;
            }
        }
        isHighlightActive = true;
    }

    private void RevertFlammableHighlight()
    {
        foreach (var kvp in originalMaterials)
            if (kvp.Key != null)
                kvp.Key.materials = kvp.Value;

        originalMaterials.Clear();
        isHighlightActive = false;
    }

    private void OnDisable()
    {
        if (isHighlightActive)
            RevertFlammableHighlight();
    }
}
