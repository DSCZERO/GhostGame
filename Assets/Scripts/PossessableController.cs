using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PossessableController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 3f;
    [Tooltip("How fast the camera rotates with mouse movement")]
    public float rotationSpeed = 200f;
    [Tooltip("Maximum upward/downward camera angle (prevents over-rotation)")]
    public float maxPitchAngle = 80f;

    // Components
    private Rigidbody rb;
    private Camera controlCamera; // Reference to the camera controlling this object
    
    // State tracking
    private bool isPossessed = false;
    
    // View control
    private float yaw = 0f;   // Horizontal rotation (left/right)
    private float pitch = 0f; // Vertical rotation (up/down)
    
    // Input caching for physics updates
    private Vector3 moveInput = Vector3.zero;

    /// <summary>
    /// Set this object as possessed or unpossessed
    /// </summary>
    /// <param name="value">True when possessed, false when released</param>
    public void SetPossessed(bool value)
    {
        isPossessed = value;

        // Lock cursor when possessed
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !value;
        
        // Reset movement when unpossessed
        if (!value)
            moveInput = Vector3.zero;
    }
    
    /// <summary>
    /// Sets the camera that will be used for this possession
    /// </summary>
    /// <param name="cam">Camera reference</param>
    public void SetCamera(Camera cam)
    {
        controlCamera = cam;
    }
    
    /// <summary>
    /// Initialize view angles for smooth transition when possessing
    /// </summary>
    /// <param name="initialYaw">Starting horizontal angle</param>
    /// <param name="initialPitch">Starting vertical angle</param>
    public void InitializeView(float initialYaw, float initialPitch)
    {
        yaw = initialYaw;
        
        // Clamp pitch to prevent strange angles
        pitch = Mathf.Clamp(initialPitch, -maxPitchAngle, maxPitchAngle);
    }

    void Start()
    {
        // Get and configure the rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate; 
            rb.freezeRotation = true; // Let us control rotation manually
        }
        else
        {
            Debug.LogError($"[PossessableController] Missing Rigidbody on {gameObject.name}");
        }
    }

    void Update()
    {
        if (!isPossessed) return;

        // --- Cache movement input ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveInput = new Vector3(h, 0, v);
        
        // Normalize input to prevent faster diagonal movement
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        // --- Handle camera look in Update for best responsiveness ---
        // Delta angles based on mouse movement
        float deltaYaw = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float deltaPitch = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        
        // Update view angles
        yaw += deltaYaw;
        pitch = Mathf.Clamp(pitch - deltaPitch, -maxPitchAngle, maxPitchAngle);

        // Apply rotation to object body (horizontal only)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        
        // Apply vertical rotation to camera
        if (controlCamera != null)
        {
            controlCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("[PossessableController] No control camera set");
        }
    }

    void FixedUpdate()
    {
        if (!isPossessed || rb == null) return;

        // --- Apply movement in FixedUpdate for smooth physics ---
        Vector3 moveWorld = transform.TransformDirection(moveInput) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveWorld);
    }
}