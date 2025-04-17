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
    public float ghostAlpha = 0.5f;               // Target transparency when in ghost mode (0 = invisible, 1 = opaque)
    public float fadeDuration = 0.5f;             // Duration of the fade effect
    
    // Add a reference to the ghost material for doors
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

    // Dictionary to store each door object and its original material
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
        // Toggle ghost mode with ghostKey
        if (Input.GetKeyDown(ghostKey))
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
        
        // Ghost mode timer and movement
        if (IsInGhostMode)
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
        IsInGhostMode = true;
        
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
        
        // Change player to Ghost layer
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        // Ignore collisions only with Door objects.
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"), 
            LayerMask.NameToLayer("Door"), 
            true
        );
        
        // Keep playerCollider enabled so that collisions with walls/floors remain active
        
        // Disable shadow casting for main and child renderers
        if (playerRenderer)
        {
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in childRenderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        // Start fade effect to ghostAlpha
        StartCoroutine(FadeToAlpha(ghostAlpha));
        
        // Change all doors to the ghost door material
        ApplyGhostDoorMaterial();
    }
    
    void ReturnToBody()
    {
        IsInGhostMode = false;
        
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
        
        // Re-enable player collider
        if (playerCollider)
        {
            playerCollider.enabled = true;
        }
        
        // Restore shadow casting for main and child renderers
        if (playerRenderer)
        {
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in childRenderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        
        // Start fade effect back to full opacity (alpha 1)
        StartCoroutine(FadeToAlpha(1f));
        
        // Revert all doors to their original materials
        RevertDoorMaterials();
    }
    
    // Force return to body when ghost time runs out
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
        
        // Calculate movement vector based on camera orientation
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        right.Normalize();
        
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Add up/down movement using jump/crouch
        if (Input.GetKey(fpc.jumpKey))
        {
            moveDirection += Vector3.up;
        }
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
    
    // Coroutine for smoothly fading all renderers to a target alpha value
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
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float blend = Mathf.Clamp01(elapsedTime / fadeDuration);
            foreach (Renderer rend in renderers)
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
            yield return null;
        }
    }
    
    /// <summary>
    /// Find all objects on the "Door" layer, store their original material,
    /// and assign the doorGhostMaterial.
    /// </summary>
    void ApplyGhostDoorMaterial()
    {
        // Clear the dictionary before applying new ghost materials
        originalDoorMaterials.Clear();

        // Find all objects in the scene
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

    /// <summary>
    /// Revert all door objects to their original materials.
    /// </summary>
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
