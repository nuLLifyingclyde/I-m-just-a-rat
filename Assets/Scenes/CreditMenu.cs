using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditMenu : MonoBehaviour
{
    [SerializeField] public GameObject CreditScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void Credit()
    {
        CreditScreen.SetActive(true);
    }

    public void Resume()
    {
        CreditScreen.SetActive(false);
    }
}
