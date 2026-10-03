using Unity.VisualScripting;
using UnityEngine;

public class FoodScript : MonoBehaviour

{
    public int foodAmount = 1;
    public GameObject gameObject;
    public MouseDirectionController mdc;
    public float multiplierboost = 1f ;
    public float durationBoost = 1f;
    

    private void OnTriggerEnter(Collider collision)
    {
        CharacterController charController = collision.GetComponent<CharacterController>();
        //Declare charactercontroller as something and find collision as character controller
        if (charController != null)
            //if there is a character controller
        {

            MouseDirectionController mdc = charController.GetComponent<MouseDirectionController>();
            //The movement code is declared it equals to the character controller finding the movement code
                if (mdc != null)
                //if there is the movement code
            {
                mdc.shakeSpeedMultiplier += multiplierboost;
                //the line that makes the speed multiply in the movement code is added or equal to this code's boost which means that when eat food, the rat speed will increase and remain with the multiplier boost
                mdc.AddFoodBoost(durationBoost, multiplierboost);
                //and add the boost from the AddFoodBoost code that has the duration and multiplier boost
               
            }

            FoodManager fm = FindFirstObjectByType<FoodManager>();

            if (fm != null)
            {
                fm.AddFood(foodAmount);
            }
            
            Destroy(gameObject);
            
        }
    }
}
