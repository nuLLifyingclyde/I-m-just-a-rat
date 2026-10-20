using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseScreen;
    public GameObject TutorialScreen;

    public void PauseStart()
    {
        PauseScreen.SetActive(true);
        TutorialScreen.SetActive(false);
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void HowToPlay()
    {
        TutorialScreen.SetActive(true);
        PauseScreen.SetActive(false);
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void HowToPlayQuit()
    {
        TutorialScreen.SetActive(false);
        PauseScreen.SetActive(true);
        if (PauseScreen.activeSelf) return;
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    public void Resume()
    {
        PauseScreen.SetActive(false);
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    public void BackToMenu(string menu)
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene(menu);
    }
}
