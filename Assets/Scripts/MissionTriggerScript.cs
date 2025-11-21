using System.Collections;
using UnityEngine;

public class MissionTriggerScript : MonoBehaviour
{


    [SerializeField] private GameObject passenger;
    AnimatorControllerDriver animatorControllerDriver;
    public MissionLogic missionLogic;
    [SerializeField] private float walkOutDistance = 2f;
    [SerializeField] private float walkOutDuration = 2f;
    [SerializeField] private bool deactivateOnFinish = true;
    // How slow the car must be (magnitude) to be considered stopped
    public float stopThreshold = 0.1f;
    // How long (seconds) the car must remain below stopThreshold while inside the trigger
    public float holdTime = 0.5f;

    // Reference to the shared StopMonitor (can be on this object or a child)
    private StopMonitor stopMonitor;

    void Awake()
    {
        animatorControllerDriver = passenger.GetComponent<AnimatorControllerDriver>();
        if (animatorControllerDriver == null)
            Debug.LogError("Animator not found on " + gameObject.name);
    }

    // Called once when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCar"))
        {
            if (stopMonitor == null) stopMonitor = GetComponentInChildren<StopMonitor>() ?? gameObject.AddComponent<StopMonitor>();

            // Start monitoring; when stopped, call StartMission with the collider
            stopMonitor.StartMonitoring(other, stopThreshold, holdTime, (c) => StartMission(c));
        }
    }

    private void StartMission(Collider other)
    {
        missionLogic.StartMission(other);
        StartCoroutine(PassengerEnterCarCoroutine(walkOutDistance, walkOutDuration));
        // freeze position of collider 'other' during walkout duration
        StartCoroutine(FreezeRigidbodyConstraintsCoroutine(other, walkOutDuration));
    }

    private IEnumerator FreezeRigidbodyConstraintsCoroutine(Collider other, float duration)
    {
        if (other == null)
            yield break;

        var rb = other.attachedRigidbody;
        if (rb == null)
            yield break;

        var previous = rb.constraints;

        // Freeze all positions and rotations
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        yield return new WaitForSeconds(duration);

        // Restore previous constraints if the Rigidbody still exists
        if (rb != null)
            rb.constraints = previous;
    }

    private IEnumerator PassengerEnterCarCoroutine(float distance, float duration)
    {
        if (passenger != null)
        {
            animatorControllerDriver.CrossfadeTo("WalkCool", 0.1f, 0);
            Vector3 dir = passenger.transform.forward.normalized;

            float elapsed = 0f;
            float speed = distance / duration;
            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                passenger.transform.position += dir * (speed * dt);
                elapsed += dt;
                yield return null;
            }

            // Make passenger disappear
            passenger.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCar"))
        {
            stopMonitor?.StopMonitoring(other);
        }
    }

    // No more local monitoring coroutine — StopMonitor handles it.
}
