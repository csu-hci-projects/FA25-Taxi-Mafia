using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        // public GameObject wheelEffectObj;
        // public ParticleSystem smokeParticle;
        public Axel axel;
    }

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 0.75f;
    public float maxSteerAngle = 45.0f;

    [Header("GTA-Style Handling")]
    [Tooltip("How much rear grip is reduced when handbrake is active (0-1)")]
    public float handbrakeGripReduction = 0.3f;
    [Tooltip("Speed at which steering becomes less sensitive (km/h)")]
    public float steeringSpeedThreshold = 80f;
    [Tooltip("Minimum steering angle at high speeds (multiplier)")]
    public float minSteeringAtSpeed = 0.4f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    public Rigidbody carRb;

    [Header("Engine Settings")]
    public float engineTorque = 1000f;       // Nm, base torque
    public float maxRPM = 6500f;            // max engine revs
    public float idleRPM = 800f;            // idle RPM
    public float[] gearRatios = { 3.2f, 2.1f, 1.5f, 1.0f, 0.8f}; // 5 gears
    public bool reversing = false;
    public float finalDrive = 3.73f;         // differential ratio
    public float shiftUpRPM = 5000f;        // shift points
    public float shiftDownRPM = 2500f;
    private float wheelRadius = 15.0f;

    [Header("Engine State")]
    public bool engineRunning = true;   // You can turn this on/off later
    public float currentRPM;
    public int currentGear = 1;            // start in 1st gear
    private float throttleInputSmooth;
    
    private float previousSteerInput = 0f;
    public bool lookBack = false;
    bool autoReverse = false;
    private bool handbrakeActive = false;
    private float[] originalSidewaysStiffness = new float[4]; // Store original friction values


    [Header("Audio")]
    public AudioSource engineAudio;
    public AudioSource tireAudio;

    // private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        SetupCarPhysics();
        carRb.centerOfMass = _centerOfMass;
        // --- Rigidbody setup for stability ---
        carRb.mass = 1150f;                        // typical sedan/muscle car
        carRb.linearDamping = 0.1f;
        carRb.angularDamping = 0.5f;
        carRb.interpolation = RigidbodyInterpolation.Interpolate;
        carRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        carRb.centerOfMass = _centerOfMass;        // tweak Y lower & slightly rear for stability

        // Store original friction values for handbrake system
        for (int i = 0; i < wheels.Count; i++)
        {
            WheelFrictionCurve forwardFriction = wheels[i].wheelCollider.forwardFriction;
            WheelFrictionCurve sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;

            if (wheels[i].axel == Axel.Front)
            {
                forwardFriction.stiffness = 2.5f;
                sidewaysFriction.stiffness = 3.0f;  // front = more grip
            }
            else // Rear
            {
                forwardFriction.stiffness = 2.5f;
                sidewaysFriction.stiffness = 2.5f;  // rear slightly lower for controlled drift
            }

            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
            
            // Store original sideways stiffness for handbrake system
            originalSidewaysStiffness[i] = sidewaysFriction.stiffness;
        }
        // carLights = GetComponent<CarLights>();
    }

    void SetupCarPhysics()
{
    if (!carRb) carRb = GetComponent<Rigidbody>();

    // ----- Rigidbody setup -----
    carRb.mass = 1400f;            // realistic sedan weight
    carRb.linearDamping = 0.05f;            // linear drag
    carRb.angularDamping = 0.3f;      // stabilize rotation on veers/jumps
    carRb.centerOfMass = _centerOfMass; // keep your center of mass

    // ----- Wheel setup -----
    foreach (var wheel in wheels)
    {
        // Forward friction
        WheelFrictionCurve fwd = wheel.wheelCollider.forwardFriction;
        fwd.stiffness = 1.8f;           // medium grip for fun slides
        fwd.extremumSlip = 0.4f;        // allows sliding before max grip
        wheel.wheelCollider.forwardFriction = fwd;

        // Sideways friction
        WheelFrictionCurve side = wheel.wheelCollider.sidewaysFriction;
        side.stiffness = 1.5f;          // lower sideways grip = driftable
        side.extremumSlip = 0.3f;
        wheel.wheelCollider.sidewaysFriction = side;

        // Suspension
        JointSpring spring = wheel.wheelCollider.suspensionSpring;
        spring.spring = 25000f;         // strong spring for off-road stability
        spring.damper = 3500f;          // dampens bounce
        wheel.wheelCollider.suspensionSpring = spring;

        wheel.wheelCollider.suspensionDistance = 0.25f; // keeps wheels grounded
    }
}

    void Update()
    {
        GetInputs();
        AnimateWheels();
        // WheelEffects();
    }

    void FixedUpdate()
    {
        GetInputs();
        Move();
        Steer();
        Brake();
        HandBrake();
        AnimateWheels();
        UpdateEngineSound();
        UpdateTireSound();
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
        // Steering stays normal
        float steer = Input.GetAxis("Horizontal");

        // Forward movement only from W
        float accel = 0f;

        if (Input.GetKey(KeyCode.W)) accel = 1f;
        if (Input.GetKey(KeyCode.S)) lookBack = true;
        else lookBack = false;

        // Reverse only if reverse key explicitly pressed (ex: LeftShift or R)
        if (Input.GetKey(KeyCode.LeftShift))
            accel = -1f;

        // store accel for physics
        moveInput = accel;

        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            // steerInput = Input.GetAxis("Horizontal");
            steerInput = Mathf.Lerp(previousSteerInput, Input.GetAxis("Horizontal"), Time.fixedDeltaTime * 5f);
            previousSteerInput = steerInput;
        }

         // Steering
        steerInput = Mathf.Lerp(previousSteerInput, Input.GetAxis("Horizontal"), Time.fixedDeltaTime * 5f);
        previousSteerInput = steerInput;

        // Forward / Reverse (S behaves like DownArrow)
        moveInput = Input.GetAxis("Vertical");

        // Auto reverse toggle (press R to enable/disable)
        if (Input.GetKeyDown(KeyCode.R))
            autoReverse = !autoReverse;
    }

    void UpdateEngineSound()
    {
        if (!engineAudio) return;

        // Pitch scales with RPM
        float pitch = Mathf.Lerp(0.8f, 2.0f, currentRPM / maxRPM);
        engineAudio.pitch = pitch;

        // Volume increases with throttle
        engineAudio.volume = Mathf.Lerp(0.2f, 1.0f, Mathf.Abs(moveInput));
    }

    void UpdateTireSound()
    {
        if (!tireAudio) return;

        float slip = Mathf.Clamp01(carRb.linearVelocity.magnitude / 50f) * Mathf.Abs(moveInput);
        tireAudio.volume = slip > 0.6f ? 1f : 0f;
    }

void Move()
{
    // Smooth throttle input
    throttleInputSmooth = Mathf.Lerp(throttleInputSmooth, Mathf.Clamp01(moveInput), Time.fixedDeltaTime * 5f);

    // Average RPM of driven wheels
    float avgWheelRPM = 0f;
    int drivenWheels = 0;
    foreach (var wheel in wheels)
    {
        if (wheel.axel == Axel.Rear)
        {
            avgWheelRPM += wheel.wheelCollider.rpm;
            drivenWheels++;
        }
    }
    if (drivenWheels > 0) avgWheelRPM /= drivenWheels;

    // Engine RPM & torque
    float gearRatio = gearRatios[Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1)] * finalDrive;
    currentRPM = Mathf.Max(idleRPM, Mathf.Abs(avgWheelRPM * gearRatio));

    if (currentRPM > shiftUpRPM && currentGear < gearRatios.Length) currentGear++;
    else if (currentRPM < shiftDownRPM && currentGear > 1) currentGear--;

    currentGear = Mathf.Clamp(currentGear, 1, gearRatios.Length);

    float torqueFactor = Mathf.Clamp01(1f - Mathf.Pow((currentRPM - (maxRPM * 0.75f)) / (maxRPM * 0.75f), 2));
    float totalTorque = engineTorque * torqueFactor * throttleInputSmooth;

    float currentSpeed = carRb.linearVelocity.magnitude;
    Vector3 localVel = carRb.transform.InverseTransformDirection(carRb.linearVelocity);
    float forwardSpeed = localVel.z; // positive = forward, negative = reverse

    // Enhanced reverse logic for J-turns
    // Allow reverse when:
    // 1. Moving backward (negative forward speed) OR
    // 2. Nearly stopped and pressing reverse
    // 3. Handbrake allows easier transition during slides
    if (moveInput < -0.1f)
    {
        // If we're already moving backward, stay in reverse
        // Or if we're slow/stopped, allow reverse
        // Or if handbrake is active (during J-turn), allow reverse transition
        if (forwardSpeed < 0.5f || currentSpeed < 2.0f || handbrakeActive)
        {
            reversing = true;
        }
    }
    // Switch to forward when:
    // 1. Moving forward (positive forward speed) OR
    // 2. Pressing forward and not in heavy reverse
    else if (moveInput > 0.1f)
    {
        if (forwardSpeed > -0.5f || currentSpeed < 2.0f || handbrakeActive)
        {
            reversing = false;
        }
    }

    foreach (var wheel in wheels)
    {
        if (wheel.axel == Axel.Rear)
        {
            float appliedTorque;
            
            if (reversing)
            {
                // Reverse torque - allow full power in reverse for J-turns
                // Use absolute value since moveInput is negative in reverse
                float reverseTorque = Mathf.Abs(moveInput) * engineTorque;
                // Boost reverse torque slightly for better reverse J-turn control
                if (handbrakeActive)
                {
                    reverseTorque *= 1.2f; // Extra power during reverse J-turn
                }
                appliedTorque = -reverseTorque; // Negative for reverse
            }
            else
            {
                appliedTorque = totalTorque * gearRatio / drivenWheels;
            }

            // --- GTA-style drift helper ---
            // Only reduce torque significantly when handbrake is active
            // Don't reduce in reverse during J-turns
            if (!handbrakeActive && !reversing)
            {
                Vector3 localVal = carRb.transform.InverseTransformDirection(carRb.linearVelocity);
                float sideSlip = Mathf.Abs(localVal.x);
                float slipFactor = Mathf.Clamp01(1f - (sideSlip / 8f)); // less aggressive reduction for normal driving
                appliedTorque *= slipFactor;
            }

            wheel.wheelCollider.motorTorque = appliedTorque;
        }
        else
        {
            wheel.wheelCollider.motorTorque = 0f;
        }
    }

    if (!reversing)
    {
        // Calculate forward RPM based on wheel speed and gear ratio
        float wheelRPM = (carRb.linearVelocity.magnitude / wheelRadius) * 60f;
        float targetRPM = wheelRPM * gearRatio * 2f;  // adjust factor as needed

        currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.fixedDeltaTime * 5f);

        // Clamp RPM so it behaves like a real car
        currentRPM = Mathf.Clamp(currentRPM, idleRPM, maxRPM);
    }


    if (reversing)
    {
        currentGear = 0;
        currentRPM = Mathf.Lerp(currentRPM, idleRPM + 1000f * Mathf.Abs(moveInput), Time.fixedDeltaTime * 5f);
    }
}

void Steer()
{
    float speed = carRb.linearVelocity.magnitude * 3.6f; // km/h

    // GTA-style: Reduce steering at high speeds but keep it responsive
    // In reverse, steering should be more responsive for J-turns
    float speedFactor = Mathf.Clamp01(speed / steeringSpeedThreshold);
    float maxAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * minSteeringAtSpeed, speedFactor);
    
    // Boost steering in reverse for better J-turn control
    if (reversing)
    {
        maxAngle *= 1.2f; // 20% more steering angle in reverse
    }

    // Smooth input - faster response when handbrake is active for J-turns
    float steerSmoothSpeed = handbrakeActive ? 8f : 5f;
    // Even faster in reverse with handbrake for reverse J-turns
    if (reversing && handbrakeActive)
    {
        steerSmoothSpeed = 10f;
    }
    
    float smoothSteer = Mathf.Lerp(previousSteerInput, steerInput, Time.fixedDeltaTime * steerSmoothSpeed);
    previousSteerInput = smoothSteer;

    foreach (var wheel in wheels)
    {
        if (wheel.axel == Axel.Front)
        {
            float targetAngle = smoothSteer * maxAngle;
            wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetAngle, Time.fixedDeltaTime * 5f);
        }
    }

    // Reduced stabilization when handbrake is active to allow controlled slides
    // Also reduce in reverse for better J-turn rotation
    if (!handbrakeActive && !reversing)
    {
        Vector3 localVel = carRb.transform.InverseTransformDirection(carRb.linearVelocity);
        float oversteerFactor = Mathf.Clamp01(Mathf.Abs(localVel.x) / 5f);
        carRb.angularVelocity *= (1f - 0.3f * oversteerFactor); // dampens rapid rotation at high side slip
    }
}


    void Brake()
    {
        bool braking = Input.GetKey(KeyCode.Space) || (moveInput < 0f && !reversing);


        foreach (var wheel in wheels)
        {
            if (braking)
            {
                wheel.wheelCollider.brakeTorque = brakeAcceleration * 1800f;
                wheel.wheelCollider.motorTorque = 0f;
            }
            else
            {
                wheel.wheelCollider.brakeTorque = 0f;
            }

        }
    }

    void HandBrake()
    {
        bool handbrakePressed = Input.GetKey(KeyCode.LeftShift);
        handbrakeActive = handbrakePressed;

        if (handbrakePressed)
        {
            // Apply brake torque to rear wheels and reduce friction for sliding
            for (int i = 0; i < wheels.Count; i++)
            {
                if (wheels[i].axel == Axel.Rear)
                {
                    wheels[i].wheelCollider.brakeTorque = brakeAcceleration * 4500f;
                    
                    // Reduce rear wheel friction for sliding (GTA-style)
                    WheelFrictionCurve sideFriction = wheels[i].wheelCollider.sidewaysFriction;
                    float originalStiffness = originalSidewaysStiffness[i];
                    sideFriction.stiffness = originalStiffness * handbrakeGripReduction;
                    wheels[i].wheelCollider.sidewaysFriction = sideFriction;
                }
            }
        }
        else
        {
            // Restore normal friction when handbrake is released
            for (int i = 0; i < wheels.Count; i++)
            {
                if (wheels[i].axel == Axel.Rear)
                {
                    wheels[i].wheelCollider.brakeTorque = 0f;
                    
                    // Restore original friction
                    WheelFrictionCurve sideFriction = wheels[i].wheelCollider.sidewaysFriction;
                    sideFriction.stiffness = originalSidewaysStiffness[i];
                    wheels[i].wheelCollider.sidewaysFriction = sideFriction;
                }
            }
        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}

// void Move()
    // {
    //     // Smooth input for realism
    //     throttleInputSmooth = Mathf.Lerp(throttleInputSmooth, Mathf.Clamp01(moveInput), Time.fixedDeltaTime * 5f);

    //     // Calculate wheel speed (average of driven wheels)
    //     float avgWheelRPM = 0f;
    //     int drivenWheels = 0;
    //     foreach (var wheel in wheels)
    //     {
    //         if (wheel.axel == Axel.Rear) // RWD example
    //         {
    //             avgWheelRPM += wheel.wheelCollider.rpm;
    //             drivenWheels++;
    //         }
    //     }
    //     if (drivenWheels > 0)
    //         avgWheelRPM /= drivenWheels;

    //     // Compute engine RPM based on wheel RPM and gear ratio
    //     // float gearRatio = gearRatios[currentGear - 1] * finalDrive;
    //     float gearRatio = gearRatios[Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1)] * finalDrive;
    //     currentRPM = Mathf.Max(idleRPM, Mathf.Abs(avgWheelRPM * gearRatio));

    //     // Gear shifting
    //     if (currentRPM > shiftUpRPM && currentGear < gearRatios.Length)
    //     {
    //         currentGear++;
    //     }
    //     else if (currentRPM < shiftDownRPM && currentGear > 1)
    //     {
    //         currentGear--;
    //     }

    //     currentGear = Mathf.Clamp(currentGear, 1, gearRatios.Length);


    //     // Engine torque curve (simple parabolic falloff near redline)
    //     float torqueFactor = Mathf.Clamp01(1f - Mathf.Pow((currentRPM - (maxRPM * 0.75f)) / (maxRPM * 0.75f), 2));
    //     float totalTorque = engineTorque * torqueFactor * throttleInputSmooth;
    //     float currentSpeed = carRb.linearVelocity.magnitude;

    //    // --- REVERSE HANDLING FIX ---
    //     // float speed = carRb.linearVelocity.magnitude;

    //     // Only allow reverse when car is nearly stopped
    //     if (moveInput < -0.1f && currentSpeed < 1.0f)
    //         {
    //             reversing = true;
    //         }
    //     else if (moveInput > 0.1f)
    //         {
    //             reversing = false;
    //         }

    //     // --- APPLY TORQUE ---
    //     foreach (var wheel in wheels)
    //     {
    //         if (wheel.axel == Axel.Rear)
    //         {
    //             if (reversing)
    //             {
    //                 // Negative torque pushes backward
    //                 wheel.wheelCollider.motorTorque = moveInput * engineTorque;
    //             }
    //             else
    //             {
    //                 // Normal forward driving
    //                 wheel.wheelCollider.motorTorque = totalTorque * gearRatio / drivenWheels;
    //                 float appliedTorque = reversing ? moveInput * engineTorque : totalTorque * gearRatio / drivenWheels;

    //                 // --- DRIFT HELPER ---
    //                 Vector3 localVelocity = carRb.transform.InverseTransformDirection(carRb.velocity);
    //                 float sideSlip = Mathf.Abs(localVelocity.x);            // lateral slip
    //                 float slipFactor = Mathf.Clamp01(1f - (sideSlip / 5f)); // reduces torque when sliding sideways
    //                 appliedTorque *= slipFactor;

    //                 wheel.wheelCollider.motorTorque = appliedTorque;
    //             }
    //         }
    //         else
    //         {
    //             wheel.wheelCollider.motorTorque = 0f;
    //         }
    //     }

    //     // In Move(), after applying torque:
    //     if (reversing)
    //     {
    //         currentGear = 0; // optional: mark as reverse gear
    //         currentRPM = Mathf.Lerp(currentRPM, idleRPM + 1000f * Mathf.Abs(moveInput), Time.fixedDeltaTime * 5f);
    //     }

    //     // Optional top speed limiter
    //     // float currentSpeed = carRb.linearVelocity.magnitude * 3.6f; // m/s to km/h
    //     // float topSpeed = 220f;
    //     // if (currentSpeed > topSpeed)
    //     // {
    //     //     foreach (var wheel in wheels)
    //     //         wheel.wheelCollider.motorTorque = 0f;
    //     // }
    // }

    // void Steer()
    // {
    //     float currentSpeed = carRb.linearVelocity.magnitude * 3.6f; // km/h
    //     float speedFactor = Mathf.Clamp01(currentSpeed / 50f); // how fast we're going
    //     float adjustedSteerAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.5f, speedFactor); // less steer at high speed
    //     float steerSmoothFactor = Mathf.Lerp(10f, 5f, currentSpeed / 50f);
    //     // Speed-based steering sensitivity
    //     float steerSensitivitySpeed = (currentSpeed > 120f) ? 0.2f :
    //                               (currentSpeed > 60f) ? 0.5f :
    //                               0.75f;

    //     steerInput = Mathf.Lerp(previousSteerInput, Input.GetAxis("Horizontal"), Time.fixedDeltaTime * steerSmoothFactor);
    //     previousSteerInput = steerInput;

    //     // inside Steer()
    //     // if (currentSpeed > 120f)
    //     // {
    //     //     turnSensitivity = 0.2f;
    //     // }
    //     // else if (currentSpeed > 60f)
    //     // {
    //     //     turnSensitivity = 0.5f;
    //     // }
    //     // else
    //     // {
    //     //     turnSensitivity = 0.75f; // your base
    //     // }

    //     foreach (var wheel in wheels)
    //     {
    //         if (wheel.axel == Axel.Front)
    //         {
    //             float targetAngle = steerInput * turnSensitivity * adjustedSteerAngle;
    //             wheel.wheelCollider.steerAngle = Mathf.Lerp(
    //                 wheel.wheelCollider.steerAngle,
    //                 targetAngle,
    //                 Time.fixedDeltaTime * 5f
    //             );
    //         }
    //     }
    // }