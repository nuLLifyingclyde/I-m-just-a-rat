using UnityEngine;
using UnityEngine.SceneManagement;

public class Trashbag : MonoBehaviour
{
    public string RatTag = "Rat";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rat"))
        {
            SceneManager.LoadScene("Trash alleyway");

            Debug.Log("Player is Dead");
        }
    }
}
