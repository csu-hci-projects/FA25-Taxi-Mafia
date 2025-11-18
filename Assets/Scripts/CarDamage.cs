using UnityEngine;

public class CarDamage : MonoBehaviour
{
  public HUDManager hud;         // drag your HUDManager here
  public float damageMultiplier = 0.1f; // tweak to taste

  void OnCollisionEnter(Collision collision)
  {
    Debug.Log("Collision detected");
    // How hard the hit was
    float force = collision.impulse.magnitude;

    // Convert force to damage
    float damage = force * damageMultiplier;

    if (damage > 0f && hud != null)
    {
      Debug.Log("Taking damage: " + damage);
      hud.TakeDamage(damage);
    }
  }
}
