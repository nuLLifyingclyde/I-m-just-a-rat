using UnityEngine;

public class Close : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    // Update is called once per frame
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Byebeye");
    }
}
