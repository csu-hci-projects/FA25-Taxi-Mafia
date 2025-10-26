
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
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    [Header("Engine Settings")]
    public float engineTorque = 700f;       // Nm, base torque
    public float maxRPM = 6000f;            // max engine revs
    public float idleRPM = 800f;            // idle RPM
    public float[] gearRatios = {3.2f, 2.1f, 1.5f, 1.0f, 0.8f }; // 5 gears
    private bool reversing = false;
    public float finalDrive = 3.7f;         // differential ratio
    public float shiftUpRPM = 6500f;        // shift points
    public float shiftDownRPM = 2500f;

    private int currentGear = 1;            // start in 1st gear
    private float currentRPM;
    private float throttleInputSmooth;

    [Header("Audio")]
    public AudioSource engineAudio;
    public AudioSource tireAudio;

    // private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;

        foreach (var wheel in wheels)
        {
            var forwardFriction = wheel.wheelCollider.forwardFriction;
            forwardFriction.stiffness = 2.0f; // higher = more grip
            wheel.wheelCollider.forwardFriction = forwardFriction;

            var sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
            sidewaysFriction.stiffness = 3.2f;
            wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
        }
        // carLights = GetComponent<CarLights>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        // WheelEffects();
    }

    void FixedUpdate()
    {
        Move();
        UpdateEngineSound();
        UpdateTireSound();
        Steer();
        Brake();
        HandBrake();
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
        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }
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
        // Smooth input for realism
        throttleInputSmooth = Mathf.Lerp(throttleInputSmooth, Mathf.Clamp01(moveInput), Time.fixedDeltaTime * 5f);

        // Calculate wheel speed (average of driven wheels)
        float avgWheelRPM = 0f;
        int drivenWheels = 0;
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear) // RWD example
            {
                avgWheelRPM += wheel.wheelCollider.rpm;
                drivenWheels++;
            }
        }
        if (drivenWheels > 0)
            avgWheelRPM /= drivenWheels;

        // Compute engine RPM based on wheel RPM and gear ratio
        // float gearRatio = gearRatios[currentGear - 1] * finalDrive;
        float gearRatio = gearRatios[Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1)] * finalDrive;
        currentRPM = Mathf.Max(idleRPM, Mathf.Abs(avgWheelRPM * gearRatio));

        // Gear shifting
        if (currentRPM > shiftUpRPM && currentGear < gearRatios.Length)
        {
            currentGear++;
        }
        else if (currentRPM < shiftDownRPM && currentGear > 1)
        {
            currentGear--;
        }

        currentGear = Mathf.Clamp(currentGear, 1, gearRatios.Length);


        // Engine torque curve (simple parabolic falloff near redline)
        float torqueFactor = Mathf.Clamp01(1f - Mathf.Pow((currentRPM - (maxRPM * 0.75f)) / (maxRPM * 0.75f), 2));
        float totalTorque = engineTorque * torqueFactor * throttleInputSmooth;
        float currentSpeed = carRb.linearVelocity.magnitude;

       // --- REVERSE HANDLING FIX ---
        // float speed = carRb.linearVelocity.magnitude;

        // Only allow reverse when car is nearly stopped
        if (moveInput < -0.1f && currentSpeed < 1.0f)
            {
                reversing = true;
            }
        else if (moveInput > 0.1f)
            {
                reversing = false;
            }

        // --- APPLY TORQUE ---
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear)
            {
                if (reversing)
                {
                    // Negative torque pushes backward
                    wheel.wheelCollider.motorTorque = moveInput * engineTorque;
                }
                else
                {
                    // Normal forward driving
                    wheel.wheelCollider.motorTorque = totalTorque * gearRatio / drivenWheels;
                }
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0f;
            }
        }

        // In Move(), after applying torque:
        if (reversing)
        {
            currentGear = 0; // optional: mark as reverse gear
            currentRPM = Mathf.Lerp(currentRPM, idleRPM + 1000f * Mathf.Abs(moveInput), Time.fixedDeltaTime * 5f);
        }

        // Optional top speed limiter
        // float currentSpeed = carRb.linearVelocity.magnitude * 3.6f; // m/s to km/h
        // float topSpeed = 220f;
        // if (currentSpeed > topSpeed)
        // {
        //     foreach (var wheel in wheels)
        //         wheel.wheelCollider.motorTorque = 0f;
        // }
    }

    void Steer()
    {
        float currentSpeed = carRb.linearVelocity.magnitude * 3.6f; // km/h
        float speedFactor = Mathf.Clamp01(currentSpeed / 120f); // how fast we're going
        float adjustedSteerAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.2f, speedFactor); // less steer at high speed

        // inside Steer()
        if (currentSpeed > 120f)
        {
            turnSensitivity = 0.4f;
        }
        else if (currentSpeed > 60f)
        {
            turnSensitivity = 0.6f;
        }
        else
        {
            turnSensitivity = 0.75f; // your base
        }
        
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                float targetAngle = steerInput * turnSensitivity * adjustedSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(
                    wheel.wheelCollider.steerAngle,
                    targetAngle,
                    Time.fixedDeltaTime * 5f
                );
            }
        }
    }

    void Brake()
    {
        bool braking = Input.GetKey(KeyCode.Space) || (moveInput < 0f && !reversing);


        foreach (var wheel in wheels)
        {
            if (braking)
            {
                wheel.wheelCollider.brakeTorque = brakeAcceleration * 1000f;
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
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (var wheel in wheels)
                if (wheel.axel == Axel.Rear)
                    wheel.wheelCollider.brakeTorque = brakeAcceleration * 3000f;
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
