using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pyrokinesis : MonoBehaviour
{
    // This boolean tracks whether the player has "heat" stored.
    private bool heat = false;

    // Maximum distance for the raycast to check for objects.
    public float rayDistance = 5f;

    void Update()
    {
        // Check if the player pressed the E key.
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Create a ray from the center of the screen (assumes a perspective camera)
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            // Perform the raycast.
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                // If the hit object is tagged as "Fire" and the player does not already have heat
                if (hit.transform.CompareTag("Fire") && !heat)
                {
                    // Get the Renderer component to change its color.
                    Renderer rend = hit.transform.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material.color = Color.white;
                    }
                    // Store the heat.
                    heat = true;
                }
                // If the player has heat and the hit object is on the "Flammable" layer.
                else if (heat && hit.transform.gameObject.layer == LayerMask.NameToLayer("Flammable"))
                {
                    // Destroy the flammable object.
                    Destroy(hit.transform.gameObject);
                    // Optionally, reset the heat once it has been used.
                    heat = false;
                }
            }
        }
    }
}
