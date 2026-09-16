using UnityEngine;

public class ShitMechanic : MonoBehaviour
{
    public FoodManager fm;
    public string RatTag = "Rat";
    public int decreaseamount = 1;
    public bool isInRange = false;

    private void Update()
    {
        if (isInRange && Input.GetMouseButtonDown(1))
        {
            fm.foodcount -= decreaseamount;
            Debug.Log("Taking a shit");
        }
    }
    private void OnTriggerEnter (Collider other)
    {
       if (other.CompareTag(RatTag))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(RatTag))
        {
            isInRange = false;
        }
    }
}
