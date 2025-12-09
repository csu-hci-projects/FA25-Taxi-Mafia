using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuContainer;
    public GameObject taxiIcon;
    public GameObject girlNPCIcon;
    public GameObject bossNPCIcon;
    public GameObject demonNPCIcon;
    public GameObject chineseNPCIcon;
    public GameObject bearNPCIcon;
    public GameObject cultistNPCIcon;
    public GameObject galacticNPCIcon;
    public GameObject mysteriousNPCIcon;
    public GameObject endZone1;
    public GameObject endZone2;
    public GameObject endZone3;
    public GameObject endZone4;
    public GameObject endZone5;
    public GameObject endZone6;
    public GameObject endZone7;


    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuContainer != null)
        {
            pauseMenuContainer.SetActive(false);
            taxiIcon.SetActive(false);
            girlNPCIcon.SetActive(false);
            bossNPCIcon.SetActive(false);
            demonNPCIcon.SetActive(false);
            bearNPCIcon.SetActive(false);
            galacticNPCIcon.SetActive(false);
            mysteriousNPCIcon.SetActive(false);
            cultistNPCIcon.SetActive(false);
            chineseNPCIcon.SetActive(false);
            endZone1.SetActive(false);
            endZone2.SetActive(false);
            endZone3.SetActive(false);
            endZone4.SetActive(false);
            endZone5.SetActive(false);
            endZone6.SetActive(false);
            endZone7.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenuContainer != null)
        {
            pauseMenuContainer.SetActive(true);
            taxiIcon.SetActive(true);
            girlNPCIcon.SetActive(true);
            bossNPCIcon.SetActive(true);
            demonNPCIcon.SetActive(true);
            bearNPCIcon.SetActive(true);
            galacticNPCIcon.SetActive(true);
            mysteriousNPCIcon.SetActive(true);
            cultistNPCIcon.SetActive(true);
            chineseNPCIcon.SetActive(true);
            endZone1.SetActive(true);
            endZone2.SetActive(true);
            endZone3.SetActive(true);
            endZone4.SetActive(true);
            endZone5.SetActive(true);
            endZone6.SetActive(true);
            endZone7.SetActive(true);
        }
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (pauseMenuContainer != null)
        {
            pauseMenuContainer.SetActive(false);
            taxiIcon.SetActive(false);
            girlNPCIcon.SetActive(false);
            bossNPCIcon.SetActive(false);
            demonNPCIcon.SetActive(false);
            bearNPCIcon.SetActive(false);
            galacticNPCIcon.SetActive(false);
            mysteriousNPCIcon.SetActive(false);
            cultistNPCIcon.SetActive(false);
            chineseNPCIcon.SetActive(false);
            endZone1.SetActive(false);
            endZone2.SetActive(false);
            endZone3.SetActive(false);
            endZone4.SetActive(false);
            endZone5.SetActive(false);
            endZone6.SetActive(false);
            endZone7.SetActive(false);
        }
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}