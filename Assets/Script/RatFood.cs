using UnityEngine;


public class RatFood : MonoBehaviour
{
    public FoodManager fm;
    public float maxfoodcounter = 60f;
    public MouseDirectionController mdc;

    public float firstboost = 0.5f;
   

    public float firstaccel = 0.5f;
  


    public float currentspeed;
    public float currentaccel;
    public float shakeCurrentSpeed;
   
   

    void Start()
    {
        currentspeed = mdc.moveSpeed;
        currentaccel = mdc.acceleration;
        shakeCurrentSpeed = mdc.shakeSpeedMultiplier;

    }
    void Update() //I changed food count to food manager
    {
        if (fm.foodcount >= 40) //20+ = fat
        {
            mdc.acceleration += firstaccel;
            mdc.moveSpeed += firstboost;
            
            Debug.Log("First Boost");
        }

       
    }
}
