using UnityEngine;

public class ApplicationClose : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Game Closed");
        }
    }
}