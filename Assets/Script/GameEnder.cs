using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnder : MonoBehaviour
{
    public FoodManager fm;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Rat"))
        {
            return;
        }

        if (fm.foodcount < 15)
        {
            Debug.Log("Player is Dead");
            SceneManager.LoadScene("Trash alleyway");
        }

        else
        {
            Debug.Log("Game complete!");
            Application.Quit();
        }
    }
}