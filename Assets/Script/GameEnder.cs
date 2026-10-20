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

        if (fm.foodcount < 40)
        {
            Debug.Log("Player is Dead");
            SceneManager.LoadScene("You Lose");
        }

        else
        {
            Debug.Log("Player eat enough food");
            SceneManager.LoadScene("You Win");
        }
    }
}