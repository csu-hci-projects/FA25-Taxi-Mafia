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

    public void DeactivateAllEndzones()
    {
        if (missionEndzones == null) return;
        foreach (GameObject endzone in missionEndzones)
        {
            if (endzone != null)
            {
                endzone.SetActive(false);
            }
        }
    }
}
