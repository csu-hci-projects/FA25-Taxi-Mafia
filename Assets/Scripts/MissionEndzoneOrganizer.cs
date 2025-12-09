using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionEndzoneOrganizer : MonoBehaviour
{
    [SerializeField] List<GameObject> missionEndzones = new List<GameObject>();

    void Awake()
    {
        foreach (GameObject endzone in missionEndzones)
        {
            endzone.SetActive(false);
        }
    }

    public GameObject GetRandomMissionEndzone()
    {
        if (missionEndzones == null || missionEndzones.Count == 0) return null;
        return missionEndzones[Random.Range(0, missionEndzones.Count)];
    }

    // Returns the world position of the currently active endzone GameObject.
    // If no endzone is active, returns null.
    public Vector3? GetActiveEndzonePosition()
    {
        if (missionEndzones == null || missionEndzones.Count == 0) return null;

        foreach (GameObject endzone in missionEndzones)
        {
            if (endzone != null && endzone.activeInHierarchy)
            {
                return endzone.transform.position;
            }
        }

        return null;
    }
}
