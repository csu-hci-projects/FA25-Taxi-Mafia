using UnityEngine;

public class MissionEndTriggerScript : MonoBehaviour
{

    public MissionLogic missionLogic;
    public float stopThreshold = 0.1f;
    public float holdTime = 0.5f;

    private StopMonitor stopMonitor;

    // Called once when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        // Example: check tag
        if (other.CompareTag("PlayerCar"))
        {
            if (stopMonitor == null) stopMonitor = GetComponentInChildren<StopMonitor>() ?? gameObject.AddComponent<StopMonitor>();
            stopMonitor.StartMonitoring(other, stopThreshold, holdTime, (c) => missionLogic.EndMission(c));
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
