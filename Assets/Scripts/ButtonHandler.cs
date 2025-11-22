using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public void OnButtonClicked()
    {
        Debug.Log("Button Clicked!");
        SceneManager.LoadScene("WorldMission");
        // Add your desired actions here
    }
}