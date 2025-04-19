using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // for Image

public class GhostMode : MonoBehaviour
{
    [Header("Ghost Mode Settings")]
    public KeyCode ghostKey = KeyCode.G;
    public float maxGhostTime = 30f;
    private float currentGhostTime;
    public float ghostSpeed = 7f;
    public float returnDistance = 1.5f;
    private bool isInNoGhostZone = false;

    [Header("Ghost Visual Effects")]
    public float ghostAlpha = 0.5f;
    public float fadeDuration = 0.5f;
    public Material doorGhostMaterial;

    [Header("Screen Feedback")]
    [Tooltip("Full‑screen UI Image (covering the screen)")]
    public Image screenOverlay;
    [Tooltip("Color to flash on entry (e.g. a pale white)")]
    public Color enterFadeColor = Color.white;
    [Tooltip("Color to flash on exit (e.g. a soft black)")]
    public Color exitFadeColor = Color.black;
    [Tooltip("How long the screen flash lasts (in/out combined)")]
    public float screenFadeTime = 0.5f;

    [Header("Physical Body")]
    public GameObject bodyPrefab;
    private GameObject bodyInstance;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip enterGhostClip;
    public AudioClip exitGhostClip;

    // internal refs
    private FirstPersonController fpc;
    private Rigidbody rb;
    private Camera playerCamera;
    private Vector3 bodyPosition;
    private Quaternion bodyRotation;
    public bool IsInGhostMode { get; private set; } = false;
    private Collider playerCollider;
    private Renderer playerRenderer;
    private Dictionary<GameObject, Material> originalDoorMaterials = new Dictionary<GameObject, Material>();

    void Start()
    {
        fpc = GetComponent<FirstPersonController>();
        rb = GetComponent<Rigidbody>();
        playerCamera = fpc.playerCamera;
        playerCollider = GetComponent<Collider>();
        playerRenderer = GetComponent<Renderer>();
        currentGhostTime = maxGhostTime;

        if (screenOverlay != null)
        {
            var c = screenOverlay.color;
            c.a = 0f;
            screenOverlay.color = c;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(ghostKey))
        {
            if (!IsInGhostMode && currentGhostTime > 0 && !isInNoGhostZone)
                EnterGhostMode();
            else if (IsInGhostMode && IsNearBody())
                ReturnToBody();
        }

        if (IsInGhostMode)
        {
            currentGhostTime -= Time.deltaTime;
            HandleGhostMovement();
            if (currentGhostTime <= 0f)
                ForceReturnToBody();
        }
        else if (currentGhostTime < maxGhostTime)
        {
            currentGhostTime = Mathf.Clamp(currentGhostTime + Time.deltaTime * 0.5f, 0f, maxGhostTime);
        }
    }

    void EnterGhostMode()
    {
        IsInGhostMode = true;

        // save body
        bodyPosition = transform.position;
        bodyRotation = transform.rotation;

        // spawn body placeholder
        if (bodyPrefab != null)
            bodyInstance = Instantiate(bodyPrefab, bodyPosition, bodyRotation);
        else
        {
            bodyInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyInstance.transform.SetPositionAndRotation(bodyPosition, bodyRotation);
            bodyInstance.transform.localScale = transform.localScale;
            var col = bodyInstance.GetComponent<Collider>();
            if (col) col.enabled = false;
        }

        // physics + collisions
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"),
            LayerMask.NameToLayer("Door"),
            true
        );

        // shadows off
        if (playerRenderer)
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // fade body
        StartCoroutine(FadeToAlpha(ghostAlpha));

        // recolor doors
        ApplyGhostDoorMaterial();

        // screen flash
        if (screenOverlay != null)
            StartCoroutine(ScreenFlash(enterFadeColor));

        // play enter sound
        if (audioSource != null && enterGhostClip != null)
            audioSource.PlayOneShot(enterGhostClip);
    }

    void ReturnToBody()
    {
        IsInGhostMode = false;

        transform.SetPositionAndRotation(bodyPosition, bodyRotation);
        if (bodyInstance) Destroy(bodyInstance);

        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        gameObject.layer = LayerMask.NameToLayer("Default");
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Ghost"),
            LayerMask.NameToLayer("Door"),
            false
        );

        if (playerCollider) 
            playerCollider.enabled = true;
        if (playerRenderer)
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        StartCoroutine(FadeToAlpha(1f));
        RevertDoorMaterials();

        // screen flash
        if (screenOverlay != null)
            StartCoroutine(ScreenFlash(exitFadeColor));

        // play exit sound
        if (audioSource != null && exitGhostClip != null)
            audioSource.PlayOneShot(exitGhostClip);
    }

    void ForceReturnToBody()
    {
        ReturnToBody();
        Debug.Log("Ghost time depleted, forced return to body");
    }

    void HandleGhostMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 fwd = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        right.y = 0f; right.Normalize();
        Vector3 dir = (fwd * v + right * h).normalized;

        if (Input.GetKey(fpc.jumpKey))   dir += Vector3.up;
        if (Input.GetKey(fpc.crouchKey)) dir += Vector3.down;

        rb.velocity = dir * ghostSpeed;
    }

    bool IsNearBody() =>
        Vector3.Distance(transform.position, bodyPosition) <= returnDistance;

    IEnumerator FadeToAlpha(float targetAlpha)
    {
        var renderers = new List<Renderer>();
        if (playerRenderer != null) renderers.Add(playerRenderer);
        renderers.AddRange(GetComponentsInChildren<Renderer>());

        var originals = new Dictionary<Renderer, Color>();
        foreach (var rend in renderers)
            originals[rend] = rend.material.color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            foreach (var rend in renderers)
            {
                Color c = originals[rend];
                c.a = Mathf.Lerp(originals[rend].a, targetAlpha, t);
                rend.material.color = c;
            }
            yield return null;
        }
    }

    IEnumerator ScreenFlash(Color flashColor)
    {
        float half = screenFadeTime * 0.5f;
        float timer = 0f;
        // fade IN
        while (timer < half)
        {
            timer += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, timer / half);
            screenOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, a);
            yield return null;
        }
        // fade OUT
        timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, timer / half);
            screenOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, a);
            yield return null;
        }
    }

    void ApplyGhostDoorMaterial()
    {
        originalDoorMaterials.Clear();
        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            if (obj.layer == LayerMask.NameToLayer("Door"))
            {
                var rend = obj.GetComponent<Renderer>();
                if (rend != null && doorGhostMaterial != null)
                {
                    originalDoorMaterials[obj] = rend.material;
                    rend.material = doorGhostMaterial;
                }
            }
        }
    }

    void RevertDoorMaterials()
    {
        foreach (var kvp in originalDoorMaterials)
        {
            if (kvp.Key != null)
            {
                var rend = kvp.Key.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = kvp.Value;
            }
        }
        originalDoorMaterials.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NoGhostZone"))
        {
            isInNoGhostZone = true;
            if (IsInGhostMode) ForceReturnToBody();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NoGhostZone"))
            isInNoGhostZone = false;
    }
}
