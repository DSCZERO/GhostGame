using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PossessableController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 200f;

    private Rigidbody rb;
    private bool isPossessed = false;

    // input cache
    private Vector3 rawInput;
    private float yaw;
    private float pitch;

    public void SetPossessed(bool value)
    {
        isPossessed = value;

        // lock cursor when possessed
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        rb.freezeRotation = true; // let us control rotation manually
    }

    void Update()
    {
        if (!isPossessed) return;

        // --- Cache movement input ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        rawInput = new Vector3(h, 0, v);
        if (rawInput.sqrMagnitude > 1f)
            rawInput.Normalize(); // avoid faster diagonal

        // --- Handle look in Update for best responsiveness ---
        yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // apply rotation to player body
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        // apply rotation to camera
        if (Camera.main != null)
            Camera.main.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void FixedUpdate()
    {
        if (!isPossessed) return;

        // --- Move in FixedUpdate for smooth physics ---
        Vector3 moveWorld = transform.TransformDirection(rawInput) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveWorld);
    }
}
