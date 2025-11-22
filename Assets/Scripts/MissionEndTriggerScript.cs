using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class MissionEndTriggerScript : MonoBehaviour
{

    MissionLogic missionLogic;

    public float stopThreshold = 0.1f;
    public float holdTime = 0.5f;

    private StopMonitor stopMonitor;

    void Awake()
    {
        missionLogic = FindAnyObjectByType<MissionLogic>();
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
        this.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCar"))
        {
            stopMonitor?.StopMonitoring(other);
        }
    }
}
