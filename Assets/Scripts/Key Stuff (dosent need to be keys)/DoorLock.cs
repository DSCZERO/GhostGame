using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorLock : MonoBehaviour
{
    [Header("Key & Door Setup")]
    [Tooltip("Must match the keyID you set on your KeyPickup")]
    public string requiredKeyID = "1";

    [Tooltip("Drag your door's Animator here (must have a Bool parameter named 'open')")]
    public Animator doorAnimator;

    [Tooltip("Name of the Bool parameter in your Animator")]
    public string openBoolName = "open";

    [Header("Click Interaction")]
    [Tooltip("Layer mask for all clickable doors (set this to your 'Doors' layer)")]
    public LayerMask doorLayerMask;
    [Tooltip("Max distance you can click from")]
    public float interactRange = 5f;

    // Prevent opening when player is in ghost form
    private GhostMode ghostMode;

    private bool doorOpened = false;
    private int openBoolHash;

    private void Awake()
    {
        openBoolHash = Animator.StringToHash(openBoolName);

        // Find GhostMode on the player via the main camera
        if (Camera.main != null)
            ghostMode = Camera.main.GetComponentInParent<GhostMode>();

        if (ghostMode == null)
            Debug.LogWarning("[DoorLock] Could not find GhostMode on player; ghost check disabled.");

        // If not manually assigned, try to get the Animator component
        if (doorAnimator == null && TryGetComponent<Animator>(out var anim))
        {
            doorAnimator = anim;
        }

        // Ensure there is a collider for raycasts
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"[{name}] needs a Collider for click detection!");
        }
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
        if (PlayerKeys.Instance.HasKey(requiredKeyID))
        {
            OpenDoor();
        }
        else
        {
            Debug.Log($"[{name}] Door is locked. You need key '{requiredKeyID}'.");
        }
    }

    private void OpenDoor()
    {
        doorOpened = true;

        if (doorAnimator != null)
        {
            // Animate door by setting the bool parameter
            doorAnimator.SetBool(openBoolHash, true);
            Debug.Log($"[{name}] Door opened via Animator.SetBool('{openBoolName}', true).");
        }
        else
        {
            // No Animator assigned or available: destroy the door
            Destroy(gameObject);
            Debug.Log($"[{name}] No Animator: door GameObject destroyed instead.");
        }
    }
}
