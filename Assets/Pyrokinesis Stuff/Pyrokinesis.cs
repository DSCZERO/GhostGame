using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;        // ← for RawImage

public class Pyrokinesis : MonoBehaviour
{
    // This boolean tracks whether the player has "heat" stored.
    private bool heat = false;

    // Maximum distance for the raycast to check for objects.
    public float rayDistance = 5f;

    [Header("Audio")]
    public AudioSource audioSource;    // assign an AudioSource (can be on this GameObject)
    public AudioClip storeHeatClip;    // sound to play when you pick up heat
    public AudioClip useHeatClip;      // sound to play when you consume it

    [Header("UI")]
    public RawImage heatIndicator;     // drag your RawImage here

    void Start()
    {
        // start with “no heat” indicator
        if (heatIndicator != null)
            heatIndicator.color = Color.black;
    }

    void Update()
    {
        // when player presses E…
        if (Input.GetKeyDown(KeyCode.E))
        {
            // cast from center of screen
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0)
            );
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                // if we hit a Fire object and we don't already have heat…
                if (hit.transform.CompareTag("Fire") && !heat)
                {
                    // turn it white
                    var rend = hit.transform.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material.color = Color.white;

                    // store heat
                    heat = true;

                    // play store‐heat sound
                    if (audioSource != null && storeHeatClip != null)
                        audioSource.PlayOneShot(storeHeatClip);

                    // update UI
                    if (heatIndicator != null)
                        heatIndicator.color = Color.white;
                }
                // else if we have heat and hit something flammable…
                else if (heat && hit.transform.gameObject.layer == LayerMask.NameToLayer("Flammable"))
                {
                    // destroy it
                    Destroy(hit.transform.gameObject);
                    heat = false;

                    // play use‐heat sound
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
