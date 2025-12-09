using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class MissionLogic : MonoBehaviour
{

    public TimerController timerController;
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI moneyText;

    [SerializeField] MissionEndzoneOrganizer missionEndzoneOrganizer;
    GameObject currentPassenger;
    AnimatorControllerDriver animatorControllerDriver;

    // Store starting passenger transform values (snapshot) so we can restore later
    private Vector3 startingPassengerPosition;
    private Quaternion startingPassengerRotation;

    private int currentMoney;


    private bool missionRunning;

    public bool StartMission(GameObject passenger)
    {
        if (missionRunning)
        {
            return false;
        }
        else
        {
            currentPassenger = passenger;
            animatorControllerDriver = passenger.GetComponent<AnimatorControllerDriver>();
            if (animatorControllerDriver == null)
                Debug.LogError("Animator not found on " + gameObject.name);

            // Snapshot the passenger's starting position/rotation so we can restore later
            startingPassengerPosition = passenger.transform.position;
            startingPassengerRotation = passenger.transform.rotation;
            missionEndzoneOrganizer.GetRandomMissionEndzone().SetActive(true);
            timerController.StopTimer();
            timerController.StartTimer();
            missionText.text = "To the liquor sto'!";
            missionRunning = true;
            return true;
        }

    }

    public void EndMission(Collider other)
    {
        StartCoroutine(PassengerExitCarCoroutine(2, 2, other));
    }


    private IEnumerator PassengerExitCarCoroutine(float distance, float duration, Collider other)
    {
        SetPassengerVisibility(true);
        currentPassenger.transform.position = other.transform.position;
        currentPassenger.transform.rotation = other.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
        if (currentPassenger != null)
        {
            Vector3 dir = currentPassenger.transform.forward.normalized;
            // Move passenger outwards a bit so the model isn't inside of the car.
            currentPassenger.transform.position += dir * 1f;
            currentPassenger.SetActive(true);

            animatorControllerDriver.CrossfadeTo("WalkCool", 0.1f, 0);
            float elapsed = 0f;
            float speed = distance / duration;
            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                currentPassenger.transform.position += dir * (speed * dt);
                elapsed += dt;
                yield return null;
            }

            animatorControllerDriver.CrossfadeTo("Idle", 0.1f, 0);

            timerController.PauseTimer();
            missionRunning = false;
            missionText.text = "";
            int missionMoney = Random.Range(100, 500);
            currentMoney += missionMoney;
            UpdateMoneyDisplay();
            // Restore passenger transform to the saved starting position/rotation
            currentPassenger.transform.position = startingPassengerPosition;
            currentPassenger.transform.rotation = startingPassengerRotation;
            currentPassenger = null;
        }
    }

    public void SetPassengerVisibility(bool visible)
    {
        if (currentPassenger == null)
            return;

        // Toggle all Renderer types (MeshRenderer, SkinnedMeshRenderer, SpriteRenderer...)
        var renderers = currentPassenger.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            r.enabled = visible;

        // Handle UI CanvasRenderers (set alpha to 0 when invisible)
        var canvasRenderers = currentPassenger.GetComponentsInChildren<UnityEngine.CanvasRenderer>(true);
        foreach (var cr in canvasRenderers)
            cr.SetAlpha(visible ? 1f : 0f);

        // Toggle particle systems' emission module so effects are hidden when invisible
        var particleSystems = currentPassenger.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            var em = ps.emission;
            em.enabled = visible;
        }
    }

    public void DeductRespawnCost(int amount)
    {
        currentMoney -= amount;
        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText == null) return;

        moneyText.text = "$" + currentMoney.ToString();

        // Change color to red if money is less than zero
        if (currentMoney < 0)
        {
            moneyText.color = Color.red;
        }
        else
        {
            moneyText.color = Color.white; // Reset to white if money is positive
        }
    }

    public void CancelMission()
    {
        if (!missionRunning) return;

        Debug.Log("[RESPAWN] Cancelling mission due to player death");
        
        // Stop the timer
        if (timerController != null)
        {
            timerController.StopTimer();
        }
        
        // Clear mission text
        if (missionText != null)
        {
            missionText.text = "";
        }
        
        // Deactivate all mission endzones
        if (missionEndzoneOrganizer != null)
        {
            missionEndzoneOrganizer.DeactivateAllEndzones();
        }
        
        // Reset passenger if there is one
        if (currentPassenger != null)
        {
            // Restore passenger to starting position
            currentPassenger.transform.position = startingPassengerPosition;
            currentPassenger.transform.rotation = startingPassengerRotation;
            SetPassengerVisibility(true);
            currentPassenger = null;
        }
        
        missionRunning = false;
    }
}
