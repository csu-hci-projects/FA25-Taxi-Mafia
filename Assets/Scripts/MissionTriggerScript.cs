using UnityEngine;

public class MissionTriggerScript : MonoBehaviour
{

    public MissionLogic missionLogic;

    // Called once when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger: " + other.tag);
        // Example: check tag
        if (other.CompareTag("Car"))
        {
            Debug.Log("maybe");
            missionLogic.StartMission(other);
        }
    }
}
