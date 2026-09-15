using UnityEngine;

public class RatFood : MonoBehaviour
{
    public FoodManager fm;
    public float maxfoodcounter = 20f;
    public MouseDirectionController mdc;
    public float slowspeed = 2f;
    public float accelerationSlow = 20f;
    public float shakeslowspeed = 0.7f;
    public float currentspeed;
    public float currentaccel;
    public float shakeCurrentSpeed;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        currentspeed = mdc.moveSpeed;
        currentaccel = mdc.acceleration;
        shakeCurrentSpeed = mdc.shakeSpeedMultiplier;


        if (other.gameObject.CompareTag("Food"))
        {
            fm.foodcount++;
        }

        if (fm.foodcount > maxfoodcounter)
        {
           

            mdc.acceleration = accelerationSlow;
            mdc.moveSpeed = slowspeed;
            mdc.shakeSpeedMultiplier = shakeslowspeed;
            Debug.Log("rat is too fat");
        }

        if (fm.foodcount < maxfoodcounter)
        {
            mdc.acceleration = currentaccel;
            mdc.moveSpeed = currentaccel;
            mdc.shakeSpeedMultiplier = shakeCurrentSpeed;
        }

       
        
    }
}
