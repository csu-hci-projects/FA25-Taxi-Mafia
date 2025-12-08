// using UnityEngine;

// public class EngineSoundAdv : MonoBehaviour
// {
//     public Rigidbody carRb;
//     public CarController carController;

//     [Header("Sources")]
//     public AudioSource startSource;  
//     public AudioSource idleSource;   
//     public AudioSource shiftSource;  // <- one-shot gear shift sounds

//     [Header("Clips")]
//     public AudioClip startClip;
//     public AudioClip idleClip;
//     public AudioClip[] gearShiftClips;

//     bool engineStarted = false;
//     float startTimer = 0f;

//     public int currentGear = 0;
//     private int lastGear = 0;

//     void Start()
//     {
//         // Start sound setup
//         startSource.loop = false;
//         startSource.clip = startClip;
//         startSource.Play();

//         // Idle setup
//         idleSource.loop = true;
//         idleSource.clip = idleClip;
//         idleSource.volume = 0f; // silent until start finishes
//         idleSource.Play();

//         engineStarted = true;

//         lastGear = carController.currentGear;

//     }

//     void Update()
//     {
//         // Handle start -> idle transition
//         if (engineStarted)
//         {
//             startTimer += Time.deltaTime;

//             if (startTimer >= startClip.length * 0.95f)
//             {
//                 idleSource.volume = 1f;
//             }
//         }

//         // Detect gear changes
//         if (carController.currentGear != lastGear)
//         {
//             PlayGearShift(carController.currentGear);
//             lastGear = carController.currentGear;
//         }
//     }

//     void PlayGearShift(int gear)
//     {
//         if (gearShiftClips.Length == 0) return;

//         int index = Mathf.Clamp(gear, 0, gearShiftClips.Length - 1);

//         shiftSource.PlayOneShot(gearShiftClips[index]);
//     }
// }


// using UnityEngine;

// public class EngineSoundAdv : MonoBehaviour
// {
//     public Rigidbody carRb;
//     public CarController carController;

//     [Header("Audio Sources")]
//     public AudioSource startSource;
//     public AudioSource idleSource;
//     public AudioSource shiftSource;   // plays once

//     [Header("Shift Sounds")]
    
//     public AudioClip[] shiftClips;    // one per gear

//     [Header("Settings")]
//     public float maxSpeed = 80f;
//     public float blendPoint = 10f;

//     private bool startPlayed = false;
//     private bool idleStarted = false;

//     private int lastGear = -1;

//     void Start()
//     {
//         if (startSource != null)
//         {
//             startSource.loop = false;
//             startSource.Play();
//             startPlayed = true;
//         }

//         if (idleSource != null)
//             idleSource.loop = true;

//         if (revSource != null)
//             revSource.loop = true;

//         if (carController != null)
//             lastGear = carController.currentGear;
//     }

//     void Update()
//     {
//         if (carRb == null || carController == null) return;

//         // --- Idle starts after start sound finishes ---
//         if (startPlayed && !idleStarted && startSource != null && !startSource.isPlaying)
//         {
//             idleSource.Play();
//             revSource.Play();
//             idleStarted = true;
//         }

//         if (!idleStarted) return;

//         // --- Gear shifting ---
//         if (carController.currentGear != lastGear)
//         {
//             PlayShiftSound(carController.currentGear);
//             lastGear = carController.currentGear;
//         }

//         // --- Blend idle/rev based on speed ---
//         float speed = carRb.linearVelocity.magnitude;
//         float t = Mathf.Clamp01(speed / blendPoint);

//         idleSource.volume = 1f - t;
//         revSource.volume = t;
//         revSource.pitch = Mathf.Lerp(0.8f, 1.8f, speed / maxSpeed);
//     }

//     void PlayShiftSound(int newGear)
//     {
//         if (shiftSource == null || shiftClips.Length == 0)
//             return;

//         int index = Mathf.Clamp(newGear - 1, 0, shiftClips.Length - 1);

//         if (shiftClips[index] != null)
//         {
//             shiftSource.clip = shiftClips[index];
//             shiftSource.Play();
//         }
//     }
// }


using UnityEngine;

public class EngineSoundAdv : MonoBehaviour
{
    public Rigidbody carRb;

    [Header("Audio Sources")]
    public AudioSource startSource;   // plays ONCE, no loop
    public AudioSource idleSource;    // looping idle
    public AudioSource revSource;     // looping rev sound

    [Header("Settings")]
    public float blendPoint = .0001f;    // speed where rev begins to dominate
    public float maxSpeed = 110f;

    private bool hasStarted = false;
    private bool idleStarted = false;

    void Start()
    {
        // --- ENGINE START (one-time) ---
        if (startSource != null)
        {
            startSource.loop = false;
            startSource.Play();
            hasStarted = true;
        }

        if (idleSource != null)
    {
        idleSource.loop = true;
        // Don't play yet if you want it after start sound
        // idleSource.Play();
    }

    if (revSource != null)
    {
        revSource.loop = true;
        revSource.Play();   // play once
        revSource.Pause();  // immediately pause so we can unpause later
    }
    }

    void Update()
{
    if (carRb == null) return;

    float speed = carRb.linearVelocity.magnitude;

    // --- Start + Idle ---
    if (hasStarted && !idleStarted)
    {
        if (!startSource.isPlaying)
        {
            idleSource.loop = true;
            idleSource.Play();
            idleStarted = true;
        }
        return; // wait until idle started
    }

    if (!idleStarted) return; // still waiting for start clip

    // --- REV LOGIC ---
     if (speed > 0.1f)
    {
        if (!revSource.isPlaying || revSource.time == 0f) // if never started
            revSource.Play();      // start playing first time
        else
            revSource.UnPause();   // resume
    }
    else
    {
        if (revSource.isPlaying)
            revSource.Pause();
    }


    // --- BLENDING ---
    float t = Mathf.Clamp01(speed / blendPoint);
    idleSource.volume = 1f - t;   // fade idle out
    revSource.volume = t;         // fade rev in

    // Optional: pitch scaling
    // revSource.pitch = Mathf.Lerp(0.9f, 1.8f, speed / maxSpeed);
}

    // void Update()
    // {
    //     if (carRb == null) return;

    //     float speed = carRb.linearVelocity.magnitude;

    //     // Wait for start sound to finish before enabling idle
    //     if (hasStarted && !idleStarted)
    //     {
    //         if (!startSource.isPlaying)
    //         {
    //             // Start the idle loop
    //             idleSource.loop = true;
    //             idleSource.Play();
    //             idleStarted = true;

    //             // Also start the rev loop muted
    //             revSource.loop = true;
    //             revSource.volume = 0f;
    //             revSource.Play();
                
    //         }
    //         return;
    //     }

    //     if (!idleStarted) return; // still waiting for start clip

    //     // --- BLENDING LOGIC ---
    //     float t = Mathf.Clamp01(speed / blendPoint);

    //     idleSource.volume = 1f - t;   // fade idle out
    //     revSource.volume = t;         // fade rev in

    //     // --- Pitch scaling ---
    //     // revSource.pitch = Mathf.Lerp(0.9f, 1.8f, speed / maxSpeed);
    // }
}


// // using System.Collections;
// // using UnityEngine;

// // public class EngineSoundAdv : MonoBehaviour
// // {
// //      public CarController car;
// //     public AudioSource startSource;
// //     public AudioSource idleSource;

// //     public AudioSource[] gearSources; // Gear1, Gear2, Gear3...
// //     public float[] gearMinRPM;        // e.g., {1000, 2000, 3000}
// //     public float[] gearMaxRPM;        // e.g., {4000, 5000, 6000}

// //     private bool started = false;

// //     void Start()
// //     {
// //         idleSource.loop = true;

// //         foreach (var s in gearSources)
// //             s.loop = true;
// //     }

// //     void Update()
// //     {
// //         if (!started && car.engineRunning)
// //         {
// //             StartCoroutine(PlayStartSound());
// //         }

// //         if (!car.engineRunning) return;

// //         float rpm = car.currentRPM;
// //         int gear = car.currentGear;

// //         // Blend idle volume (more idle at low rpm)
// //         idleSource.volume = Mathf.Clamp01(1f - (rpm - 800f) / 1200f);

// //         // Blend gear loops
// //         for (int i = 0; i < gearSources.Length; i++)
// //         {
// //             bool activeGear = (i + 1 == gear);
// //             float vol = activeGear ? 1f : 0f;
// //             gearSources[i].volume = Mathf.Lerp(gearSources[i].volume, vol, Time.deltaTime * 5f);

// //             if (activeGear)
// //             {
// //                 float t = Mathf.InverseLerp(gearMinRPM[i], gearMaxRPM[i], rpm);
// //                 gearSources[i].pitch = Mathf.Lerp(0.8f, 1.6f, t);
// //             }
// //         }
// //     }

// //     IEnumerator PlayStartSound()
// //     {
// //         started = true;

// //         startSource.Play();
// //         yield return new WaitForSeconds(startSource.clip.length);

// //         idleSource.Play();

// //         foreach (var s in gearSources)
// //             s.Play();
// //     }
// // }
