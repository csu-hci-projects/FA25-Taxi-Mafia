using UnityEngine;
using System.Collections;

public class MissionEndTriggerScript : MonoBehaviour
{

    public MissionLogic missionLogic;
    AnimatorControllerDriver animatorControllerDriver;

    [SerializeField] private GameObject passenger;

    [SerializeField] private float walkOutDistance = 2f;
    [SerializeField] private float walkOutDuration = 2f;

    public float stopThreshold = 0.1f;
    public float holdTime = 0.5f;

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
        // Example: check tag
        if (other.CompareTag("PlayerCar"))
        {
            if (stopMonitor == null) stopMonitor = GetComponentInChildren<StopMonitor>() ?? gameObject.AddComponent<StopMonitor>();
            stopMonitor.StartMonitoring(other, stopThreshold, holdTime, (c) => EndMission(c));
        }
    }

    private void EndMission(Collider other)
    {
        missionLogic.EndMission(other);
        passenger.transform.position = other.transform.position;
        passenger.transform.rotation = other.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
        StartCoroutine(PassengerExitCarCoroutine(walkOutDistance, walkOutDuration));
    }

    private IEnumerator PassengerExitCarCoroutine(float distance, float duration)
    {
        if (passenger != null)
        {
            Vector3 dir = passenger.transform.forward.normalized;
            // Move passenger outwards a bit so the model isn't inside of the car.
            passenger.transform.position += dir * 1f;
            passenger.SetActive(true);

            animatorControllerDriver.CrossfadeTo("WalkCool", 0.1f, 0);
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
}
