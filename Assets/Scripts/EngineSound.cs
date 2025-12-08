using UnityEngine;

public class EngineSound : MonoBehaviour
{
    public Rigidbody carRb;

    [Header("Audio Sources")]
    public AudioSource idleSource;   // looping idle clip
    public AudioSource revSource;    // looping rev clip

    [Header("Settings")]
    public float maxSpeed = 80f;

    [Tooltip("Speed at which rev sound becomes dominant")]
    public float blendPoint = 10f;

    void Start()
    {
        if (idleSource != null)
        {
            idleSource.loop = true;
            idleSource.Play();
        }

        if (revSource != null)
        {
            revSource.loop = true;
            revSource.Play();
        }
    }

    void Update()
    {
        if (carRb == null) return;

        float speed = carRb.linearVelocity.magnitude;

        // Normalized 0-1 value for blending
        float t = Mathf.Clamp01(speed / blendPoint);

        // Fade idle OUT as rev fades IN
        idleSource.volume = 1f - t;
        revSource.volume = t;

        // Adjust pitch of rev sound for realism
        revSource.pitch = Mathf.Lerp(0.8f, 1.8f, speed / maxSpeed);
    }
}


// using UnityEngine;

// public class EngineSound : MonoBehaviour
// {
//     public Rigidbody carRb;

//     public AudioSource idleSource;
//     public AudioSource revSource;

//     public float maxSpeed = 140f;

//     void Start()
//     {
//         idleSource.loop = true;
//         revSource.loop = true;

//         idleSource.Play();
//         revSource.Play();
//     }

//     void Update()
//     {
//         float speed = carRb.linearVelocity.magnitude;
//         float t = Mathf.Clamp01(speed / maxSpeed);

//         // Blend volumes
//         idleSource.volume = 1f - t;
//         revSource.volume = t;

//         // Optional: pitch shift for rev source
//         revSource.pitch = Mathf.Lerp(1f, 2f, t);
//     }
// }


// using UnityEngine;

// public class EngineSound : MonoBehaviour
// {
//     public Rigidbody carRb;
//     public AudioSource audioSource;

//     [Header("Sound Settings")]
//     public float minPitch = 0.8f;
//     public float maxPitch = 2.0f;
//     public float maxSpeed = 150f;

//     void Update()
//     {
//         if (carRb == null || audioSource == null) return;

//         float speed = carRb.linearVelocity.magnitude;

//         // normalize speed to 0-1
//         float t = Mathf.Clamp01(speed / maxSpeed);

//         // adjust engine pitch
//         audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);

//         // make sure engine starts playing once
//         if (!audioSource.isPlaying)
//         {
//             audioSource.Play();
//         }
//     }
// }
