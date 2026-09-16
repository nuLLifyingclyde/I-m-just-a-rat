using UnityEngine;
using UnityEngine.SceneManagement;

public class Trashbag : MonoBehaviour
{
    public string RatTag = "Rat";

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
