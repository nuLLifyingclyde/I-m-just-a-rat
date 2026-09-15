using UnityEngine;

public class ShitMechanic : MonoBehaviour
{
    public FoodManager fm;
    public string RatTag = "Rat";
    public int decreaseamount = 1;
    public bool isInRange = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Update()
    {

        if (isInRange && Input.GetMouseButtonDown(1))
        {
            fm.foodcount -= decreaseamount;
            Debug.Log("Taking a shit");
        }

    }

    // Update is called once per frame
    void OnTriggerEnter (Collider other)
    {
       if (other.CompareTag(RatTag))
        {
            isInRange = true;
        }
    }

     void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(RatTag))
        {
            isInRange = false;
        }
    }
}
