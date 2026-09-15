using UnityEngine;

public class RatFood : MonoBehaviour
{
    public FoodManager fm;
    public MouseDirectionController mdc;
    public float slowspeed = 2f;
    public float accelerationSlow = 20f;
    public float shakeslowspeed = 0.7f;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        


        if (other.gameObject.CompareTag("Food"))
        {
            fm.foodcount++;
        }

        if (fm.foodcount > 10)
        {
           

            mdc.acceleration = accelerationSlow;
            mdc.moveSpeed = slowspeed;
            mdc.shakeSpeedMultiplier = shakeslowspeed;
            Debug.Log("rat is too fat");
        }

       
        
    }
}
