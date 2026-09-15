using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorOpen DO;
    public string RatTag = "Rat";
  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DO.isRotating = false;
        
    }

    // Update is called once per frame

    public void OnTriggerEnter(Collider collision)
    {
       
        
            DO.isRotating = true;
            Debug.Log("Door open");
        
    }
}
