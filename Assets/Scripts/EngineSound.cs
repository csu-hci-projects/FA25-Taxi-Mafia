using UnityEngine;

public class EngineSound : MonoBehaviour
{
    public Rigidbody carRb;
    public AudioSource audioSource;

    [Header("Sound Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2.0f;
    public float maxSpeed = 150f;

    void Update()
    {
        if (carRb == null || audioSource == null) return;

        float speed = carRb.linearVelocity.magnitude;

        // normalize speed to 0-1
        float t = Mathf.Clamp01(speed / maxSpeed);

        // adjust engine pitch
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);

        // make sure engine starts playing once
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
