using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseScreen;
    public GameObject TutorialScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PauseStart()
    {
        PauseScreen.SetActive(true);
        Time.timeScale = 0;
    }

   public void HowToPlay()
    {
        TutorialScreen.SetActive(true);
        Time.timeScale = 0;
    }

    public void HowToPlayQuit()
    {
        TutorialScreen.SetActive(false);
        Time.timeScale = 1;
    }

    // Update is called once per frame
    public void Resume()
    {
        PauseScreen.SetActive(false);
        Time.timeScale = 1;
    }
}
