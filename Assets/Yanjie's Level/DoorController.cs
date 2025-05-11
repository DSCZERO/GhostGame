using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public float interactDistance = 5.0f; // Distance at which player can interact with door
    public float moveDistance = 4.0f; // Distance the door moves upward
    public float moveDuration = 2.0f; // Time taken for door to move
    
    [Header("Activation Requirements")]
    [Tooltip("Number of buttons required to activate this door")]
    public int requiredButtonsCount = 1;
    
    // State
    private int activatedButtonsCount = 0;
    private bool isDoorOpen = false;
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private GhostMode playerGhostMode;
    
    void Start()
    {
        // Save initial position
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.up * moveDistance;
        
        // Find player's GhostMode component
        if (Camera.main != null)
            playerGhostMode = Camera.main.GetComponentInParent<GhostMode>();
        
        if (playerGhostMode == null)
            Debug.LogWarning("GhostMode component not found, door functionality may be limited");
    }
    
    void Update()
    {
        // Check if player is nearby, presses E, can open door, and isn't in ghost mode
        if (!isDoorOpen && 
            Input.GetKeyDown(KeyCode.E) && 
            IsPlayerNearby() && 
            IsPlayerHuman() && 
            CanOpenDoor())
        {
            OpenDoor();
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
    
    public void RegisterButtonActivation()
    {
        activatedButtonsCount++;
        Debug.Log($"Button activated ({activatedButtonsCount}/{requiredButtonsCount})");
    }
    
    private bool CanOpenDoor()
    {
        return activatedButtonsCount >= requiredButtonsCount;
    }
    
    private void OpenDoor()
    {
        if (isDoorOpen) return;
        
        isDoorOpen = true;
        StartCoroutine(MoveDoorRoutine());
    }
    
    private IEnumerator MoveDoorRoutine()
    {
        float elapsed = 0;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            // Smoothly move the door
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            yield return null;
        }
        
        // Ensure door reaches final position
        transform.position = targetPosition;
    }
}