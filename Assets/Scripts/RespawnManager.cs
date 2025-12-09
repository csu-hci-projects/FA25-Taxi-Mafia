using System.Collections;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnDelay = 10f;
    
    [Header("References")]
    public GameObject carPrefab; // Assign the car prefab in the editor
    public Transform respawnPoint; // Optional: specific respawn point. If null, uses initial position
    
    private GameObject currentCar;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isRespawning = false;
    
    private HUDManager hudManager;
    private MissionLogic missionLogic;
    
    void Start()
    {
        // Find the car in the scene
        currentCar = GameObject.FindGameObjectWithTag("PlayerCar");
        
        if (currentCar != null)
        {
            // Save initial position and rotation
            initialPosition = currentCar.transform.position;
            initialRotation = currentCar.transform.rotation;
        }
        else if (respawnPoint != null)
        {
            // Use respawn point if car not found
            initialPosition = respawnPoint.position;
            initialRotation = respawnPoint.rotation;
        }
        
        // Validate prefab assignment
        if (carPrefab == null)
        {
            Debug.LogWarning("[RESPAWN] Car Prefab is not assigned! Please assign the car prefab in the inspector. Respawn will not work without it.");
        }
        else
        {
            Debug.Log("[RESPAWN] Car Prefab assigned: " + carPrefab.name);
        }
        
        // Find other managers
        hudManager = FindAnyObjectByType<HUDManager>();
        missionLogic = FindAnyObjectByType<MissionLogic>();
    }
    
    public void OnPlayerDeath()
    {
        if (isRespawning) 
        {
            Debug.Log("[RESPAWN] Already respawning, ignoring death event");
            return; // Prevent multiple respawn coroutines
        }
        
        Debug.Log("[RESPAWN] Player died, starting respawn in " + respawnDelay + " seconds");
        isRespawning = true;
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        Debug.Log("[RESPAWN] Respawn delay complete, respawning player");
        
        // Deduct money (100 dollars)
        if (missionLogic != null)
        {
            missionLogic.DeductRespawnCost(100);
        }
        else
        {
            Debug.LogWarning("[RESPAWN] MissionLogic not found, cannot deduct money");
        }
        
        // Respawn the car
        RespawnCar();
        
        isRespawning = false;
    }
    
    private void RespawnCar()
    {
        // Determine spawn position and rotation
        Vector3 spawnPosition = respawnPoint != null ? respawnPoint.position : initialPosition;
        Quaternion spawnRotation = respawnPoint != null ? respawnPoint.rotation : initialRotation;
        
        Debug.Log("[RESPAWN] Attempting to respawn car at position " + spawnPosition);
        Debug.Log("[RESPAWN] carPrefab is " + (carPrefab != null ? carPrefab.name : "NULL"));
        
        // If car prefab is assigned, instantiate it
        if (carPrefab != null)
        {
            Debug.Log("[RESPAWN] Instantiating car from prefab");
            currentCar = Instantiate(carPrefab, spawnPosition, spawnRotation);
            
            if (currentCar == null)
            {
                Debug.LogError("[RESPAWN] Failed to instantiate car from prefab!");
                return;
            }
        }
        else
        {
            Debug.LogWarning("[RESPAWN] No car prefab assigned, trying to find existing car in scene");
            // Try to find car by tag (in case it wasn't destroyed)
            currentCar = GameObject.FindGameObjectWithTag("PlayerCar");
            
            if (currentCar == null)
            {
                Debug.LogError("[RESPAWN] Cannot respawn car - no prefab assigned and no car found in scene! Please assign the car prefab in the RespawnManager component.");
                return;
            }
            
            Debug.Log("[RESPAWN] Found existing car, resetting position");
            // Reset position and rotation
            currentCar.transform.position = spawnPosition;
            currentCar.transform.rotation = spawnRotation;
            
            // Reset rigidbody
            Rigidbody rb = currentCar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
        // Safety check - make sure we have a car
        if (currentCar == null)
        {
            Debug.LogError("[RESPAWN] currentCar is null after respawn attempt! This should not happen.");
            return;
        }
        
        // Get the car's rigidbody once
        Rigidbody carRb = currentCar.GetComponent<Rigidbody>();
        
        // Reconnect HUDManager references if needed
        if (hudManager != null)
        {
            // Update HUDManager's target rigidbody
            if (carRb != null && hudManager.targetRb != carRb)
            {
                hudManager.targetRb = carRb;
            }
            
            // Reset health
            hudManager.currentHealth = hudManager.maxHealth;
            
            // Reconnect explosion reference - always update it since the old one was destroyed
            ForceExplosion explosion = currentCar.GetComponent<ForceExplosion>();
            if (explosion != null)
            {
                hudManager.explosion = explosion;
                Debug.Log("[RESPAWN] Reconnected explosion reference to new car");
            }
            else
            {
                Debug.LogWarning("[RESPAWN] No ForceExplosion component found on respawned car! Explosion will not work.");
            }
        }
        
        // Reconnect CarDamage reference if needed
        CarDamage carDamage = currentCar.GetComponent<CarDamage>();
        if (carDamage != null && carDamage.hud != hudManager)
        {
            carDamage.hud = hudManager;
        }
        
        // Reconnect CameraController references
        CameraController cameraController = FindAnyObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.carTransform = currentCar.transform;
            CarController carController = currentCar.GetComponent<CarController>();
            if (carController != null)
            {
                cameraController.carController = carController;
            }
        }
        
        // Reset rigidbody velocity and rotation
        if (carRb != null)
        {
            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("[RESPAWN] Car respawned successfully at " + spawnPosition);
    }
}

