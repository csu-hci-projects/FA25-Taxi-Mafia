using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnDelay = 10f;
    
    private MissionLogic missionLogic;
    
    void Start()
    {
        missionLogic = FindAnyObjectByType<MissionLogic>();
        
        // Restore money from previous death if it exists
        if (PlayerPrefs.HasKey("PlayerMoney"))
        {
            int savedMoney = PlayerPrefs.GetInt("PlayerMoney");
            if (missionLogic != null)
            {
                missionLogic.SetMoney(savedMoney);
            }
            // Clear the saved money so it doesn't persist on next play
            PlayerPrefs.DeleteKey("PlayerMoney");
        }
    }
    
    public void OnPlayerDeath()
    {
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        Debug.Log("[RESPAWN] Respawn delay complete, reloading scene");
        
        // Deduct money (100 dollars) and save it
        if (missionLogic != null)
        {
            int currentMoney = missionLogic.GetMoney();
            int newMoney = currentMoney - 100;
            PlayerPrefs.SetInt("PlayerMoney", newMoney);
            PlayerPrefs.Save();
        }
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
