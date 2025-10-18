using UnityEngine;

public class MissionTriggerScript : MonoBehaviour
{

    public MissionLogic missionLogic;
    // How slow the car must be (magnitude) to be considered stopped
    public float stopThreshold = 0.1f;
    // How long (seconds) the car must remain below stopThreshold while inside the trigger
    public float holdTime = 0.5f;

    // Reference to the shared StopMonitor (can be on this object or a child)
    private StopMonitor stopMonitor;

    // Called once when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCar"))
        {
            if (stopMonitor == null) stopMonitor = GetComponentInChildren<StopMonitor>() ?? gameObject.AddComponent<StopMonitor>();

            // Start monitoring; when stopped, call StartMission with the collider
            stopMonitor.StartMonitoring(other, stopThreshold, holdTime, (c) => missionLogic.StartMission(c));
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
