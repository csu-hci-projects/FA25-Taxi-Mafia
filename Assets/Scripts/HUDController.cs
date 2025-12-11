using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
  public Rigidbody targetRb;     
  public TextMeshProUGUI speedText;

  public Image healthFill;       
  public float maxHealth = 100f;
  public float currentHealth = 100f;

  float healthBarFullWidth;

  public ForceExplosion explosion;
  
  [Header("Smoke Effect")]
  public GameObject smokePrefab;
  public float smokeSpawnOffset = 2f; // Distance in front of car to spawn smoke
  private bool smokeSpawned = false;

  void Awake()
  {

    RectTransform rt = healthFill.rectTransform;
    rt.anchorMin = new Vector2(0f, 0.5f);
    rt.anchorMax = new Vector2(0f, 0.5f);
    rt.pivot = new Vector2(0f, 0.5f);

    healthBarFullWidth = rt.sizeDelta.x;
  }

  void Update()
  {
    UpdateSpeed();
    UpdateHealthBar();
  }

  private void UpdateSpeed()
  {
    if (targetRb == null) return;

    float speed = targetRb.linearVelocity.magnitude * 2.23694f;
    speedText.text = Mathf.RoundToInt(speed) + " mph";
  }

  private void UpdateHealthBar()
  {
    float pct = Mathf.Clamp01(currentHealth / maxHealth);
    RectTransform rt = healthFill.rectTransform;
    rt.sizeDelta = new Vector2(healthBarFullWidth * pct, rt.sizeDelta.y);
  }

  public void TakeDamage(float amount)
  {
    currentHealth -= amount;

    // Spawn smoke when health reaches 1/4 or below (and hasn't been spawned yet)
    if (!smokeSpawned && currentHealth <= maxHealth * 0.25f && targetRb != null && smokePrefab != null)
    {
      SpawnSmoke();
      smokeSpawned = true;
    }

    if (currentHealth <= 0)
    {
      currentHealth = 0;

      // Cancel any ongoing mission
      MissionLogic missionLogic = FindAnyObjectByType<MissionLogic>();
      if (missionLogic != null)
      {
          missionLogic.CancelMission();
      }

      // EXPLODE THE CAR DIRECTLY
      if (explosion != null)
          explosion.Explode();

      // Notify RespawnManager about death
      RespawnManager respawnManager = FindAnyObjectByType<RespawnManager>();
      if (respawnManager != null)
      {
          respawnManager.OnPlayerDeath();
      }
    }
  }
  
  private void SpawnSmoke()
  {
    if (targetRb == null || smokePrefab == null) return;
    
    // Calculate position at the front of the car
    Vector3 frontPosition = targetRb.transform.position + targetRb.transform.forward * smokeSpawnOffset;
    
    // Spawn the smoke prefab
    GameObject smokeInstance = Instantiate(smokePrefab, frontPosition, targetRb.transform.rotation);
    
    // Make the smoke a child of the car so it follows it
    smokeInstance.transform.SetParent(targetRb.transform);
  }
}
