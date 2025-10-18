using System;
using TMPro;
using UnityEngine;

public class MissionLogic : MonoBehaviour
{

    public TimerController timerController;

    public void StartMission(Collider other)
    {
        timerController.StopTimer();
        timerController.StartTimer();
    }

    
    public void EndMission(Collider other)
    {
        timerController.PauseTimer();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
