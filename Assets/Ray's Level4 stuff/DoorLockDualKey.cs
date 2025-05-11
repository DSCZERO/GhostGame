using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class DoorLockDualKey : MonoBehaviour
{
    [Header("Key & Scene Setup")]
    [Tooltip("First key ID required to unlock the door (must match your KeyPickup keyID)")]
    public string requiredKeyID1 = "Key1";

    [Tooltip("Second key ID required to unlock the door (must match your KeyPickup keyID)")]
    public string requiredKeyID2 = "Key2";

    [Tooltip("Name of the scene to load once both keys are used")]
    public string sceneToLoad = "NextScene";

    [Header("Click Interaction")]
    [Tooltip("Layer mask for all clickable doors (set this to your 'Doors' layer)")]
    public LayerMask doorLayerMask;

    [Tooltip("Max distance you can click from")]
    public float interactRange = 5f;

    // Prevent interaction when player is in ghost form
    private GhostMode ghostMode;

    private bool doorOpened = false;

    private void Awake()
    {
        // Find GhostMode on the player via the main camera
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[DoorLockDualKey] Could not find GhostMode on player; ghost check disabled.");

        // Ensure there is a collider for raycasts
        if (GetComponent<Collider>() == null)
            Debug.LogError($"[{name}] needs a Collider for click detection!");
    }

    private void Update()
    {
        if (doorOpened) return;

        // Do not allow opening while in ghost mode
        if (ghostMode != null && ghostMode.IsInGhostMode)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, doorLayerMask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    TryOpen();
                }
            }
        }
    }

    private void TryOpen()
    {
        // Check if player has both required keys
        if (PlayerKeys.Instance.HasKey(requiredKeyID1) && PlayerKeys.Instance.HasKey(requiredKeyID2))
        {
            doorOpened = true;
            Debug.Log($"[{name}] Both keys found. Loading scene '{sceneToLoad}'.");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log($"[{name}] Door is locked. You need keys '{requiredKeyID1}' and '{requiredKeyID2}'.");
        }
    }
}
