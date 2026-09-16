using UnityEngine;


public class RatFood : MonoBehaviour
{
    public FoodManager fm;
    public float maxfoodcounter = 20f;
    public MouseDirectionController mdc;

    public float slowspeed = 2f;
    public float accelerationSlow = 20f;
    public float shakeslowspeed = 0.7f;

    public float mediumspeed = 3f;
    public float accelerationMedium = 25f;
    public float mediumShakesspeed = 0.85f;


    public float currentspeed;
    public float currentaccel;
    public float shakeCurrentSpeed;
   
    // Update is called once per frame
    /*void OnTriggerEnter(Collider other)
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
    }*/

    void Start()
    {
        currentspeed = mdc.moveSpeed;
        currentaccel = mdc.acceleration;
        shakeCurrentSpeed = mdc.shakeSpeedMultiplier;

    }
    void Update() //I changed food count to food manager
    {
        if (fm.foodcount >= maxfoodcounter) //20+ = fat
        {
            mdc.acceleration = accelerationSlow;
            mdc.moveSpeed = slowspeed;
            mdc.shakeSpeedMultiplier = shakeslowspeed;
            Debug.Log("Rat is too fat");
        }

        else if (fm.foodcount >= 12) //5-9 = slower
        {
            mdc.acceleration = accelerationMedium;
            mdc.moveSpeed = mediumspeed;
            mdc.shakeSpeedMultiplier = mediumShakesspeed;
            Debug.Log("Be careful!");
        }

        else //0-4 = normal
        {
            mdc.acceleration = currentaccel;
            mdc.moveSpeed = currentspeed;
            mdc.shakeSpeedMultiplier = shakeCurrentSpeed;
        }
    }
}
