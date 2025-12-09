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
}
