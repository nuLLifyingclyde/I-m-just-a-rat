using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnder : MonoBehaviour
{
    public FoodManager fm;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (fm.foodcount < 15)
        {
            SceneManager.LoadScene("Trash alleyway");

            Debug.Log("Player is Dead");
        }

       
    }
}
