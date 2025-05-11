using System.Collections;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header("Button Settings")]
    public float interactDistance = 2.5f; // Distance at which player can interact with button
    public float colorChangeSpeed = 2.0f; // Speed of color transition
    
    [Header("References")]
    [Tooltip("Door to be activated by this button")]
    public DoorController targetDoor;
    
    // Color settings
    private Color deactivatedColor = Color.red;
    private Color activatedColor = Color.green;
    
    // State
    private bool isActivated = false;
    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private GhostMode playerGhostMode;
    
    void Start()
    {
        // Get renderer and material
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer == null)
        {
            Debug.LogError("Button is missing Renderer component!");
            return;
        }
        
        // Save material reference and set initial color
        buttonMaterial = buttonRenderer.material;
        buttonMaterial.color = deactivatedColor;
        
        // Find player's GhostMode component
        if (Camera.main != null)
            playerGhostMode = Camera.main.GetComponentInParent<GhostMode>();
        
        if (playerGhostMode == null)
            Debug.LogWarning("GhostMode component not found, button functionality may be limited");
    }
    
    void Update()
    {
        // Check if player is nearby, presses E, and isn't in ghost mode
        if (!isActivated && 
            Input.GetKeyDown(KeyCode.E) && 
            IsPlayerNearby() && 
            IsPlayerHuman())
        {
            ActivateButton();
        }
    }
    
    private bool IsPlayerNearby()
    {
        if (Camera.main == null) return false;
        
        // Get player position (from camera's parent)
        Transform playerTransform = Camera.main.transform.parent;
        if (playerTransform == null) return false;
        
        // Calculate distance
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactDistance;
    }
    
    private bool IsPlayerHuman()
    {
        // If GhostMode component not found, default to allowing interaction
        if (playerGhostMode == null) return true;
        
        // Only allow interaction when not in ghost mode
        return !playerGhostMode.IsInGhostMode;
    }
    
    private void ActivateButton()
    {
        isActivated = true;
        
        // Start color transition
        StartCoroutine(ChangeColorRoutine());
        
        // Notify target door that this button has been activated
        if (targetDoor != null)
            targetDoor.RegisterButtonActivation();
    }
    
    private IEnumerator ChangeColorRoutine()
    {
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime * colorChangeSpeed;
            buttonMaterial.color = Color.Lerp(deactivatedColor, activatedColor, t);
            yield return null;
        }
        
        // Ensure final color is correct
        buttonMaterial.color = activatedColor;
    }
}