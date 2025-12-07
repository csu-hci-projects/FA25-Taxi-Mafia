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
        }
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}