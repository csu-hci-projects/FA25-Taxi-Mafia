using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform target; // Assign your car here
    public Transform pivot;  // The pivot object
    public Camera cam;

    [Header("Orbit Settings")]
    public float mouseSensitivity = 4f;
    public float minY = -20f;
    public float maxY = 60f;
    public float defaultPitch = 10f;   // fallback pitch when not looking
    public float resetSpeed = 6f;      // how fast we return to default when RMB released
    
    float yaw;
    float pitch;

    [Header("Follow Settings")]
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;
    public Vector3 followOffset = new Vector3(0, 2f, -6f);

    private Rigidbody targetRigidbody;
    private Vector3 velocity = Vector3.zero;
    private float verticalVelocity; // for damping vertical bob

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pitch = defaultPitch;
        yaw = 0f;

        // Get rigidbody if available for smoother position tracking
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody>();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        bool isLooking = Input.GetMouseButton(1); // right mouse button

        // --- MOUSE LOOK (only while right button held) ---
        if (isLooking)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        else
        {
            // smoothly return to default chase angle when not looking
            float resetFactor = 1f - Mathf.Exp(-resetSpeed * Time.deltaTime);
            yaw = Mathf.LerpAngle(yaw, 0f, resetFactor);
            pitch = Mathf.Lerp(pitch, defaultPitch, resetFactor);
        }

        pitch = Mathf.Clamp(pitch, minY, maxY);

        // --- GET SMOOTH CAR POSITION ---
        // Use rigidbody position if available (respects interpolation), otherwise use transform
        Vector3 targetPosition = targetRigidbody != null ? targetRigidbody.position : target.position;

        // Lightly damp vertical changes to reduce bobbing
        float verticalDamp = 1f - Mathf.Exp(-followSpeed * 0.5f * Time.deltaTime);
        targetPosition.y = Mathf.Lerp(transform.position.y, targetPosition.y, verticalDamp);

        // Use SmoothDamp for jitter-free movement (better than Lerp for physics-based objects)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            1f / followSpeed,
            Mathf.Infinity,
            Time.deltaTime
        );

        // --- ROTATE PIVOT RELATIVE TO CAR'S ROTATION ---
        // Get car's forward direction and apply mouse offset
        float carYaw = target.eulerAngles.y;
        float finalYaw = carYaw + yaw; // yaw is relative offset
        
        Quaternion targetRotation = Quaternion.Euler(pitch, finalYaw, 0);
        
        // Smoothly rotate the pivot
        if (pivot != null)
        {
            // Use exponential smoothing for smooth rotation
            float rotationSmoothFactor = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
            pivot.rotation = Quaternion.Slerp(
                pivot.rotation,
                targetRotation,
                rotationSmoothFactor
            );

            // Keep pivot aligned to rig position to avoid drift
            pivot.position = transform.position;
        }

        // --- CAMERA OFFSET BEHIND PIVOT ---
        if (cam != null && pivot != null)
        {
            // Apply offset in the pivot's local space
            cam.transform.position = pivot.position + pivot.TransformDirection(followOffset);
            cam.transform.LookAt(pivot.position);
        }
    }
}
