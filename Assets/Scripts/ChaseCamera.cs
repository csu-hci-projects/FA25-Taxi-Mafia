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
    public float defaultPitch = 10f;   // default chase angle
    public float resetSpeed = 6f;      // how fast we return to default when RMB released
    
    float yaw;
    float pitch;

    [Header("Follow Settings")]
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;
    public Vector3 followOffset = new Vector3(0, 2f, -6f);

    private Rigidbody targetRigidbody;

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

        // --- FOLLOW CAR POSITION SMOOTHLY ---
        // Use rigidbody position if available (respects interpolation), otherwise use transform
        Vector3 targetPosition = targetRigidbody != null ? targetRigidbody.position : target.position;
        
        // Simple exponential smoothing - works well and is stable
        float smoothFactor = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothFactor
        );

        // --- ROTATE PIVOT RELATIVE TO CAR'S ROTATION ---
        // Get car's rotation - use rigidbody if available for smoother rotation tracking
        float carYaw;
        if (targetRigidbody != null)
        {
            carYaw = targetRigidbody.rotation.eulerAngles.y;
        }
        else
        {
            carYaw = target.eulerAngles.y;
        }
        
        float finalYaw = carYaw + yaw; // yaw is relative offset
        Quaternion targetRotation = Quaternion.Euler(pitch, finalYaw, 0);
        
        // Smoothly rotate the pivot
        if (pivot != null)
        {
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
