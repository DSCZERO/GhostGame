using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMode : MonoBehaviour
{
    [Header("Ghost Mode Settings")]
    public KeyCode ghostKey = KeyCode.G;          // Key to enter/exit ghost mode
    public float maxGhostTime = 30f;              // Maximum duration of ghost mode
    public float currentGhostTime;                // Current remaining ghost time
    public float ghostSpeed = 7f;                 // Ghost movement speed
    public float returnDistance = 1.5f;           // How close you need to be to return to your body
    
    [Header("Physical Body")]
    public GameObject bodyPrefab;                 // Optional body prefab
    private GameObject bodyInstance;              // Instantiated body
    
    // Internal references
    private FirstPersonController fpc;            // Reference to first person controller
    private Rigidbody rb;                         // Reference to rigidbody
    private Camera playerCamera;                  // Reference to player camera
    private Vector3 bodyPosition;                 // Body position
    private Quaternion bodyRotation;              // Body rotation
    private bool isInGhostMode = false;           // Current ghost mode state
    private Collider playerCollider;              // Player collider
    private Renderer playerRenderer;              // Player renderer

    void Start()
    {
        // Get component references
        fpc = GetComponent<FirstPersonController>();
        rb = GetComponent<Rigidbody>();
        playerCamera = fpc.playerCamera;
        playerCollider = GetComponent<Collider>();
        playerRenderer = GetComponent<Renderer>();
        
        // Initialize ghost time
        currentGhostTime = maxGhostTime;
    }

    void Update()
    {
        // Toggle ghost mode with G key
        if (Input.GetKeyDown(ghostKey))
        {
            // Case 1: Not in ghost mode - try to enter ghost mode
            if (!isInGhostMode && currentGhostTime > 0)
            {
                EnterGhostMode();
            }
            // Case 2: In ghost mode and near body - return to body
            else if (isInGhostMode && IsNearBody())
            {
                ReturnToBody();
            }
            // Other cases: do nothing to avoid conflicts
        }
        
        // Ghost mode timer and movement
        if (isInGhostMode)
        {
            // Decrease ghost mode time
            currentGhostTime -= Time.deltaTime;
            
            // Handle ghost movement
            HandleGhostMovement();
            
            // If time runs out, force return
            if (currentGhostTime <= 0)
            {
                ForceReturnToBody();
            }
        }
        else
        {
            // Slowly recover ghost time when not in ghost mode
            if (currentGhostTime < maxGhostTime)
            {
                currentGhostTime += Time.deltaTime * 0.5f; // Recover at half the depletion rate
                currentGhostTime = Mathf.Clamp(currentGhostTime, 0, maxGhostTime);
            }
        }
    }
    
    void EnterGhostMode()
    {
        isInGhostMode = true;
        
        // Save body position and rotation
        bodyPosition = transform.position;
        bodyRotation = transform.rotation;
        
        // Create body instance
        if (bodyPrefab != null)
        {
            bodyInstance = Instantiate(bodyPrefab, bodyPosition, bodyRotation);
        }
        else
        {
            // If no body prefab, create a simple placeholder
            bodyInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyInstance.transform.position = bodyPosition;
            bodyInstance.transform.rotation = bodyRotation;
            bodyInstance.transform.localScale = transform.localScale;
            
            // Remove capsule collider to avoid physics conflicts
            Collider bodyCollider = bodyInstance.GetComponent<Collider>();
            if (bodyCollider)
            {
                bodyCollider.enabled = false;
            }
        }
        
        // Modify player physics properties
        rb.useGravity = false;          // Disable gravity
        rb.velocity = Vector3.zero;     // Clear velocity
        
        // Ignore collisions - move player to a special layer
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        
        // Set layer collision
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ghost"), LayerMask.NameToLayer("Default"), true);
        
        // Disable player collider
        if (playerCollider)
        {
            playerCollider.enabled = false;
        }
        
        // Remove shadow - disable shadow casting
        if (playerRenderer)
        {
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        // Also remove shadows from all child renderers
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in childRenderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
    
    void ReturnToBody()
    {
        isInGhostMode = false;
        
        // Move player back to body position
        transform.position = bodyPosition;
        transform.rotation = bodyRotation;
        
        // Destroy body instance
        if (bodyInstance != null)
        {
            Destroy(bodyInstance);
        }
        
        // Restore player physics properties
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        
        // Restore collision
        gameObject.layer = LayerMask.NameToLayer("Default");
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ghost"), LayerMask.NameToLayer("Default"), false);
        
        // Enable player collider
        if (playerCollider)
        {
            playerCollider.enabled = true;
        }
        
        // Restore shadow casting
        if (playerRenderer)
        {
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        
        // Restore shadows for all child renderers
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in childRenderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
    
    // Force return to body when time runs out
    void ForceReturnToBody()
    {
        ReturnToBody();
        Debug.Log("Ghost time depleted, forced return to body");
    }
    
    void HandleGhostMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculate movement vector based on camera
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        
        // Keep vertical component for forward/backward movement
        // Only zero out y component for side-to-side movement
        right.y = 0;
        right.Normalize();
        
        // Calculate movement direction
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Add up/down movement using jump and crouch keys
        if (Input.GetKey(fpc.jumpKey)) // Jump key (usually Space)
        {
            moveDirection += Vector3.up;
        }
        if (Input.GetKey(fpc.crouchKey)) // Crouch key (usually Left Control)
        {
            moveDirection += Vector3.down;
        }
        
        // Apply movement
        rb.velocity = moveDirection * ghostSpeed;
    }
    
    // Check if player is near enough to the body to return
    bool IsNearBody()
    {
        float distanceToBody = Vector3.Distance(transform.position, bodyPosition);
        return distanceToBody <= returnDistance;
    }
}