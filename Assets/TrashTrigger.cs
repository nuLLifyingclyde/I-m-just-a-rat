using UnityEngine;

public class TrashTrigger : MonoBehaviour
{
    public GameObject trashbag1;
    public GameObject trashbag2;
    
    public string RatTag = "Rat";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        
            trashbag1.SetActive(false);
        trashbag2.SetActive(false);

    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(RatTag))
        {
            trashbag1.SetActive(true);
            trashbag2.SetActive(true);
            Debug.Log("trash is falling");

        }

    }
}
