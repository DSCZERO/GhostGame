using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    #region Camera Settings
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;
    #endregion

    #region Camera Zoom
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;
    private bool isZoomed = false;
    #endregion

    #region Movement Settings
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;
    private bool isWalking = false;
    #endregion

    #region Sprint Settings
    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    // New option for toggle sprint
    public bool toggleSprint = false; 
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;
    #endregion

    #region Jump Settings
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    private bool isGrounded = false;
    #endregion

    #region Crouch Settings
    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;
    private bool isCrouched = false;
    private Vector3 originalScale;
    
    // To communicate with GhostMode
    [HideInInspector]
    public bool isInGhostMode = false;
    #endregion

    #region Head Bob
    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);
    private Vector3 jointOriginalPos;
    private float timer = 0;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        crosshairObject = GetComponentInChildren<Image>();
        
        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {
        // Setup cursor
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Setup crosshair
        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        // Setup sprint bar
        SetupSprintBar();
    }

    void SetupSprintBar()
    {
        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if (useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Handle camera movement
        HandleCamera();
        
        // Handle jump, crouch and sprint inputs
        HandleInputs();
        
        // Update ground check
        CheckGround();
        
        // Update head bob
        if (enableHeadBob)
        {
            HeadBob();
        }
    }

    void HandleCamera()
    {
        if (cameraCanMove)
        {
            // Mouse look control
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        // Handle zoom
        if (enableZoom)
        {
            // Toggle zoom or hold to zoom based on setting
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                isZoomed = !isZoomed;
            }
            else if (holdToZoom && !isSprinting)
            {
                if (Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if (Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Smoothly transition FOV for zoom
            if (isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }
    }

    void HandleInputs()
    {
        // Handle sprint key press
        if (enableSprint)
        {
            // For toggle sprint mode
            if (toggleSprint && Input.GetKeyDown(sprintKey) && !isSprintCooldown)
            {
                isSprinting = !isSprinting;
                
                // Cancel sprint if no stamina left
                if (isSprinting && sprintRemaining <= 0)
                {
                    isSprinting = false;
                }
            }
            // For hold sprint mode
            else if (!toggleSprint)
            {
                isSprinting = Input.GetKey(sprintKey) && !isSprintCooldown && sprintRemaining > 0;
            }
            
            // Update sprint state
            UpdateSprint();
        }

        // Handle jump input
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        // Handle crouch input - only when not in ghost mode
        if (enableCrouch && !isInGhostMode)
        {
            if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
            {
                Crouch();
            }

            if (Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                isCrouched = false;
                Crouch();
            }
            else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                isCrouched = true;
                Crouch();
            }
        }
    }

    void UpdateSprint()
    {
        if (isSprinting)
        {
            isZoomed = false;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

            // Drain sprint remaining while sprinting
            if (!unlimitedSprint)
            {
                sprintRemaining -= 1 * Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    isSprinting = false;
                    isSprintCooldown = true;
                }
            }
        }
        else
        {
            // Regain sprint while not sprinting
            sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
        }

        // Handle sprint cooldown 
        if (isSprintCooldown)
        {
            sprintCooldown -= 1 * Time.deltaTime;
            if (sprintCooldown <= 0)
            {
                isSprintCooldown = false;
            }
        }
        else
        {
            sprintCooldown = sprintCooldownReset;
        }

        // Update sprint bar
        if (useSprintBar && !unlimitedSprint)
        {
            float sprintRemainingPercent = sprintRemaining / sprintDuration;
            sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            
            // Show/hide sprint bar based on settings
            if (hideBarWhenFull)
            {
                if (isSprinting)
                {
                    sprintBarCG.alpha += 5 * Time.deltaTime;
                }
                else if (sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (playerCanMove)
        {
            // Calculate movement direction
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Check if player is walking
            isWalking = (targetVelocity.x != 0 || targetVelocity.z != 0) && isGrounded;

            // Calculate speed and apply movement
            if (isSprinting)
            {
                ApplyMovement(targetVelocity, sprintSpeed);
                
                // Uncrouch when sprinting
                if (isCrouched)
                {
                    Crouch();
                }
            }
            else
            {
                ApplyMovement(targetVelocity, walkSpeed);
            }
        }
    }

    void ApplyMovement(Vector3 direction, float speed)
    {
        // Convert local direction to world space
        Vector3 targetVelocity = transform.TransformDirection(direction) * speed;
        
        // Calculate required velocity change
        Vector3 velocityChange = (targetVelocity - rb.velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        
        // Apply the force
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void Jump()
    {
        // Add force to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // Uncrouch when jumping with toggle crouch
        if (isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    void Crouch()
    {
        // Calculate height difference and offset
        float heightDifference = originalScale.y - crouchHeight;
        float offset = heightDifference / 2f;

        if (isCrouched)
        {
            // Stand up
            transform.localScale = originalScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);
            walkSpeed /= speedReduction;
            isCrouched = false;
        }
        else
        {
            // Crouch down
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            transform.position = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);
            walkSpeed *= speedReduction;
            isCrouched = true;
        }
    }

    void HeadBob()
    {
        if (isWalking)
        {
            // Calculate head bob speed based on movement state
            if (isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            
            // Apply the head bob effect
            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, 
                jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, 
                jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z
            );
        }
        else
        {
            // Reset timer and smoothly return to original position when stopped
            timer = 0;
            joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed);
        }
    }
}