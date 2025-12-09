using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class MissionLogic : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    [SerializeField] MissionEndzoneOrganizer missionEndzoneOrganizer;
    GameObject currentPassenger;
    AnimatorControllerDriver animatorControllerDriver;

    // Store starting passenger transform values (snapshot) so we can restore later
    private Vector3 startingPassengerPosition;
    private Quaternion startingPassengerRotation;

    private int currentMoney;


    private bool missionRunning;

    public Transform arrow;  // The arrow indicator
    public GameObject car;

    public AudioSource enterCarSound;
    public AudioSource exitCarSound;

    void Update()
    {
        var maybeEndzonePos = missionEndzoneOrganizer.GetActiveEndzonePosition();
        if (!maybeEndzonePos.HasValue || car == null || arrow == null)
        {
            if (arrow != null && arrow.gameObject.activeSelf)
                arrow.gameObject.SetActive(false);
            return;
        }

        // Ensure arrow is visible while we update it
        if (!arrow.gameObject.activeSelf)
            arrow.gameObject.SetActive(true);

        Vector3 endPos = maybeEndzonePos.Value;
        Vector3 carPos = car.transform.position;

        Debug.Log("car " + car.transform.position);


        // Direction from car to endzone, flattened on the horizontal plane
        Vector3 dir = endPos - carPos;

        Debug.Log("dir " + dir);

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        // Compute direction in the car's local space so 0 = forward
        Vector3 localDir = Quaternion.Inverse(car.transform.rotation) * dir.normalized;

        // Compute Z rotation so that: 0 = forward (up), 90 = left, 180 = back, 270 = right
        float zAngle = Mathf.Atan2(-localDir.x, localDir.z) * Mathf.Rad2Deg;
        zAngle = (zAngle + 360f) % 360f;

        // Apply rotation only on Z axis (suitable for 2D/UI arrow that faces "up" at 0°)
        arrow.localEulerAngles = new Vector3(0f, 0f, zAngle);
    }

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
            missionRunning = true;
            if (enterCarSound != null)
            {
                enterCarSound.loop = false;
                enterCarSound.Play();
            }
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
            if (exitCarSound != null)
            {
                exitCarSound.loop = false;
                exitCarSound.Play();
            }
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

            missionRunning = false;
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

    public int GetMoney()
    {
        return currentMoney;
    }

    public void SetMoney(int amount)
    {
        currentMoney = amount;
        UpdateMoneyDisplay();
    }

    public void CancelMission()
    {
        if (!missionRunning) return;

        Debug.Log("[RESPAWN] Cancelling mission due to player death");

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

    private void UpdateMoneyDisplay()
    {
        if (moneyText == null) return;

        moneyText.text = currentMoney.ToString() + "$";

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
}
