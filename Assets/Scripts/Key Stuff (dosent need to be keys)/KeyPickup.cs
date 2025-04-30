using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    [Tooltip("Unique ID for this key (example: 'LibraryKey')")]
    public string keyID;

    // Reference to GhostMode to prevent picking up while ghosted
    private GhostMode ghostMode;

    private void Awake()
    {
        // Find GhostMode on the player via the main camera
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[KeyPickup] Could not find GhostMode on player; ghost pickup check disabled.");
    }

    private void Update()
    {
        // Do not allow pickup while in ghost form
        if (ghostMode != null && ghostMode.IsInGhostMode)
            return;

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f)) // 5f = max click distance
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    Pickup();
                }
            }
        }
    }

    private void Pickup()
    {
        PlayerKeys.Instance.AddKey(keyID);
        Destroy(gameObject); // Remove the key from the world
    }
}
