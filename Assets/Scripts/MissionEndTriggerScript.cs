using UnityEngine;

public class MissionEndTriggerScript : MonoBehaviour
{

    public MissionLogic missionLogic;

    // Called once when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        // Example: check tag
        if (other.CompareTag("Car"))
        {
            missionLogic.EndMission(other);
        }
    }
}
