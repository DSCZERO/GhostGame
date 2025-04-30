using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyID; // Unique ID for this key (example: "LibraryKey")

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f)) // 5f = max distance you can click from
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
