using UnityEngine;

public class InstructionDestroy : MonoBehaviour
{
    public static bool hasSeenInstructions = false;
    public GameObject button;
    public GameObject foodCollectedUI;
    public GameObject boostBar;
    public GameObject foodBar;
    public GameObject foodDecleration;

    void Start()
    {
        if (hasSeenInstructions)
        {
            Time.timeScale = 1;   
            Destroy(gameObject);

            button.SetActive(true);
            foodCollectedUI.SetActive(true);
            boostBar.SetActive(true);
            foodBar.SetActive(true);
            foodDecleration.SetActive(true);
        }
        else
        {
            Time.timeScale = 0;

            button.SetActive(false);
            foodCollectedUI.SetActive(false);
            boostBar.SetActive(false);
            foodBar.SetActive(false);
            foodDecleration.SetActive(false);
        }
    }

    public void GoAway()
    {
        Time.timeScale = 1;
        hasSeenInstructions = true;
        Destroy(gameObject);

        button.SetActive(true);
        foodCollectedUI.SetActive(true);
        boostBar.SetActive(true);
        foodBar.SetActive(true);
        foodDecleration.SetActive(true);
    }
}