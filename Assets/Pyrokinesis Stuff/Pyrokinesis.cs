using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;        // ← for RawImage

public class Pyrokinesis : MonoBehaviour
{
    // Tracks whether the player has "heat" stored.
    private bool heat = false;

    // Maximum distance for the raycast to check for objects.
    public float rayDistance = 5f;

    [Header("Audio")]
    public AudioSource audioSource;    // assign an AudioSource (can be on this GameObject)
    public AudioClip storeHeatClip;    // sound to play when you pick up heat
    public AudioClip useHeatClip;      // sound to play when you consume it

    [Header("UI")]
    public RawImage heatIndicator;     // drag your RawImage here

    // Reference to GhostMode to restrict use
    private GhostMode ghostMode;

    void Awake()
    {
        // Locate the player's GhostMode via the main camera
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[Pyrokinesis] GhostMode not found on player; pyrokinesis disabled.");
    }

    void Start()
    {
        // begin with "no heat" indicator
        if (heatIndicator != null)
            heatIndicator.color = Color.black;
    }

    void Update()
    {
        // only allow when in ghost form
        if (ghostMode == null || !ghostMode.IsInGhostMode)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // cast from center of screen
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
            );
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                // pick up heat from a Fire-tagged object
                if (hit.transform.CompareTag("Fire") && !heat)
                {
                    var rend = hit.transform.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material.color = Color.white;

                    heat = true;

                    // play store-heat sound
                    if (audioSource != null && storeHeatClip != null)
                        audioSource.PlayOneShot(storeHeatClip);

                    // update UI
                    if (heatIndicator != null)
                        heatIndicator.color = Color.white;

                    // prevent re-collection by untagging
                    hit.transform.tag = "Untagged";
                }
                // consume heat on flammable-layer objects
                else if (heat && hit.transform.gameObject.layer == LayerMask.NameToLayer("Flammable"))
                {
                    // extinguish/destroy
                    Destroy(hit.transform.gameObject);
                    heat = false;

                    // play use-heat sound
                    if (audioSource != null && useHeatClip != null)
                        audioSource.PlayOneShot(useHeatClip);

                    // update UI
                    if (heatIndicator != null)
                        heatIndicator.color = Color.black;
                }
            }
        }
    }
}
