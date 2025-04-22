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
    private bool isInNoGhostZone = false;         // Tracks whether in a sacred zone

    [Header("Ghost Visual Effects")]
    public float ghostAlpha = 0.5f;               // Target transparency when in ghost mode
    public float fadeDuration = 0.5f;             // Duration of the fade effect
    public Material doorGhostMaterial;            // The material to use for doors in ghost mode

    [Header("Physical Body")]
    public GameObject bodyPrefab;                 // Optional body prefab
    private GameObject bodyInstance;              // Instantiated body

    // Internal references
    private FirstPersonController fpc;            // Reference to first person controller
    private Rigidbody rb;                         // Reference to rigidbody
    private Camera playerCamera;                  // Reference to player camera
    private Vector3 bodyPosition;                 // Body position
    private Quaternion bodyRotation;              // Body rotation
    public bool IsInGhostMode { get; private set; } = false;
    private Collider playerCollider;              // Player collider
    private Renderer playerRenderer;              // Player renderer

    // Dictionary to store door materials
    private Dictionary<GameObject, Material> originalDoorMaterials = new Dictionary<GameObject, Material>();

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
        // Toggle ghost mode
        if (Input.GetKeyDown(ghostKey))
        {
            ToggleGhostMode();
        }
        
        // Update ghost mode timer and movement
        if (IsInGhostMode)
        {
            UpdateGhostTimer();
            HandleGhostMovement();
        }
        else
        {
            RecoverGhostTime();
        }
    }
    
    void ToggleGhostMode()
    {
        // Case 1: Not in ghost mode - try to enter ghost mode
        if (!IsInGhostMode && currentGhostTime > 0 && !isInNoGhostZone)
        {
            EnterGhostMode();
        }
        // Case 2: In ghost mode and near body - return to body
        else if (IsInGhostMode && IsNearBody())
        {
            ReturnToBody();
        }
    }
    
    void UpdateGhostTimer()
    {
        // Decrease ghost mode time
        currentGhostTime -= Time.deltaTime;
        
        // If time runs out, force return
        if (currentGhostTime <= 0)
        {
            ForceReturnToBody();
        }
    }
    
    void RecoverGhostTime()
    {
        // Slowly recover ghost time when not in ghost mode
        if (currentGhostTime < maxGhostTime)
        {
            currentGhostTime += Time.deltaTime * 0.5f; // Recover at half the depletion rate
            currentGhostTime = Mathf.Clamp(currentGhostTime, 0, maxGhostTime);
        }
    }
    
    void EnterGhostMode()
    {
        IsInGhostMode = true;
        
        // Update FPC to prevent crouch behavior in ghost mode
        if (fpc != null)
        {
            fpc.isInGhostMode = true;
        }
        
        // Save body position and rotation
        bodyPosition = transform.position;
        bodyRotation = transform.rotation;
        
        // Create body instance
        CreateBodyInstance();
        
        // Modify player physics properties
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        
        // Change player to Ghost layer
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        
        // Ignore collisions only with Door objects
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"), 
            LayerMask.NameToLayer("Door"), 
            true
        );
        
        // Disable shadow casting
        SetShadowCasting(UnityEngine.Rendering.ShadowCastingMode.Off);
        
        // Start fade effect to ghostAlpha
        StartCoroutine(FadeToAlpha(ghostAlpha));
        
        // Change all doors to the ghost door material
        ApplyGhostDoorMaterial();
    }
    
    void ReturnToBody()
    {
        IsInGhostMode = false;
        
        // Update FPC to allow crouch behavior again
        if (fpc != null)
        {
            fpc.isInGhostMode = false;
        }
        
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
        
        // Restore collision: change player back to Default layer
        gameObject.layer = LayerMask.NameToLayer("Default");
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"), 
            LayerMask.NameToLayer("Door"), 
            false
        );
        
        // Restore shadow casting
        SetShadowCasting(UnityEngine.Rendering.ShadowCastingMode.On);
        
        // Start fade effect back to full opacity
        StartCoroutine(FadeToAlpha(1f));
        
        // Revert all doors to their original materials
        RevertDoorMaterials();
    }
    
    void ForceReturnToBody()
    {
        ReturnToBody();
        Debug.Log("Ghost time depleted, forced return to body");
    }
    
    void CreateBodyInstance()
    {
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
    }
    
    void SetShadowCasting(UnityEngine.Rendering.ShadowCastingMode mode)
    {
        if (playerRenderer)
        {
            playerRenderer.shadowCastingMode = mode;
        }
        
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in childRenderers)
        {
            r.shadowCastingMode = mode;
        }
    }
    
    void HandleGhostMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculate movement direction based on camera orientation
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        
        // Keep movement on horizontal plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Add vertical movement using jump/crouch keys
        // Jump key to move up
        if (Input.GetKey(fpc.jumpKey))
        {
            moveDirection += Vector3.up;
        }
        
        // Crouch key to move down - this doesn't trigger actual crouch animation
        if (Input.GetKey(fpc.crouchKey))
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
    
    // Coroutine for smoothly fading all renderers to a target alpha
    IEnumerator FadeToAlpha(float targetAlpha)
    {
        // Gather all renderers (main and children)
        List<Renderer> renderers = new List<Renderer>();
        if (playerRenderer != null)
        {
            renderers.Add(playerRenderer);
        }
        renderers.AddRange(GetComponentsInChildren<Renderer>());
        
        // Cache original colors
        Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
        foreach (Renderer rend in renderers)
        {
            originalColors[rend] = rend.material.color;
        }
        
        // Gradually fade to target alpha
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float blend = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            foreach (Renderer rend in renderers)
            {
                if (rend != null && originalColors.ContainsKey(rend))
                {
                    Color originalColor = originalColors[rend];
                    float newAlpha = Mathf.Lerp(originalColor.a, targetAlpha, blend);
                    Color newColor = new Color(
                        originalColor.r, 
                        originalColor.g, 
                        originalColor.b, 
                        newAlpha
                    );
                    rend.material.color = newColor;
                }
            }
            yield return null;
        }
    }
    
    // Find all objects on the "Door" layer and apply ghost material
    void ApplyGhostDoorMaterial()
    {
        // Clear the dictionary before applying new ghost materials
        originalDoorMaterials.Clear();

        // Find all door objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Door"))
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null && doorGhostMaterial != null)
                {
                    // Store the original material
                    originalDoorMaterials[obj] = rend.material;
                    // Assign ghost material
                    rend.material = doorGhostMaterial;
                }
            }
        }
    }

    // Revert all door objects to their original materials
    void RevertDoorMaterials()
    {
        foreach (var kvp in originalDoorMaterials)
        {
            GameObject doorObject = kvp.Key;
            Material originalMat = kvp.Value;

            if (doorObject != null)
            {
                Renderer rend = doorObject.GetComponent<Renderer>();
                if (rend != null && originalMat != null)
                {
                    rend.material = originalMat;
                }
            }
        }
        
        // Clear after reverting
        originalDoorMaterials.Clear();
    }

    // Trigger handlers for NoGhostZones
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NoGhostZone"))
        {
            isInNoGhostZone = true;

            if (IsInGhostMode)
            {
                ForceReturnToBody();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NoGhostZone"))
        {
            isInNoGhostZone = false;
        }
    }
}