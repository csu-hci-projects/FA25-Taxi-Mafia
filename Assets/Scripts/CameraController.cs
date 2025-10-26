using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // [SerializeField] private Vector3 offset;
    // [SerializeField] private Transform target;
    // [SerializeField] private float translateSpeed;
    // [SerializeField] private float rotationSpeed;

    public Transform carTransform;       // Your car
    public CarController carController;  
    public float distance = 6f;          // Distance from car
    public float height = 2f;            // Height above car
    public float smoothSpeed = 5f;       // How fast camera moves
    public float pitch = 10f;            // Slight downward angle
    public Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

     [Header("Dynamic Effects")]
    public float tiltAngle = 5f;          // camera rolls when turning
    public float pitchAccelFactor = 2f;   // pitch when accelerating/braking
    public float shakeIntensity = 0.2f;   // shake on bumps
    public float shakeSpeed = 10f;

    private Vector3 desiredPosition;
    private Quaternion desiredRotation;
    private Vector3 velocity = Vector3.zero;
    private bool reversing = false;
    private float roll = 0f;

    // Optional: get this from CarController
    // public Transform carTransform;  
    

    void LateUpdate()
    {
        if (!carTransform || !carController) return;

        reversing = carController.reversing;

        // Desired position
        Vector3 desiredPosition = reversing
            ? carTransform.position + carTransform.forward * distance + Vector3.up * height
            : carTransform.position - carTransform.forward * distance + Vector3.up * height;

        // Shake on bumps (rough approximation)
        float verticalSpeed = carTransform.GetComponent<Rigidbody>().linearVelocity.y;
        Vector3 shake = Vector3.up * Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity * Mathf.Abs(verticalSpeed);

        desiredPosition += shake;

        // Position smoothing
        float currentSmooth = reversing ? smoothSpeed * 0.5f : smoothSpeed;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / currentSmooth);

        // Base rotation
        Vector3 lookTarget = carTransform.position + lookAtOffset;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        // Roll on turning
        float turnInput = Input.GetAxis("Horizontal"); // steering input
        roll = Mathf.Lerp(roll, -turnInput * tiltAngle, Time.deltaTime * 2f);

        // Pitch on acceleration/braking
        float accelInput = Input.GetAxis("Vertical");
        float pitchOffset = -accelInput * pitchAccelFactor;

        // Apply roll and pitch
        Vector3 euler = desiredRotation.eulerAngles;
        euler.z = roll;          // roll
        euler.x += pitchOffset;   // pitch dynamic
        desiredRotation = Quaternion.Euler(euler);

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * currentSmooth);
    }
    }

    // private void FixedUpdate()
    // {
    //     HandleTranslation();
    //     HandleRotation();
    // }
   
    // private void HandleTranslation()
    // {
    //     var targetPosition = target.TransformPoint(offset);
    //     transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    // }
    // private void HandleRotation()
    // {
    //     var direction = target.position - transform.position;
    //     var rotation = Quaternion.LookRotation(direction, Vector3.up);
    //     transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    // }
// }